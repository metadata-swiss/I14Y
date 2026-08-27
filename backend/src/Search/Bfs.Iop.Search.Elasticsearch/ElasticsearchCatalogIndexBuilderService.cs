using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Data.Indexing;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Search.Abstractions;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Full (re)build of the Elasticsearch catalog index: streams the corpus from
/// <see cref="IIndexDataReader"/> and pushes it through <see cref="ICatalogIndexService"/>.
/// <para>
/// Catalog only. The codelist-entry index is rebuilt separately, by
/// <c>ICodeListEntryIndexService.BuildIndexAsync</c>; both callers of this builder invoke that
/// alongside it, so a rebuild that skipped one would leave half the search stale.
/// </para>
/// <para>
/// Reads the database through <c>Bfs.Iop.Core.Data</c> rather than the business services, which is
/// what keeps this project — and the host that runs it — clear of the business layer.
/// </para>
/// </summary>
internal sealed class ElasticsearchCatalogIndexBuilderService : IIndexBuilderService
{
    /// <summary>
    /// Resources per bulk request.
    /// <para>
    /// A catalog document is flat — this index declares no nested mappings — so this is also the
    /// document count per request, unlike the codelist index where one entry expands into several.
    /// 100 made a full rebuild hundreds of serial round trips, each waiting on the network before the
    /// next batch was even read from the database.
    /// </para>
    /// </summary>
    private const int BuildBatchSize = 1_000;

    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IIndexDataReader _reader;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly ElasticsearchClient _client;
    private readonly string _index;
    private readonly ILogger<ElasticsearchCatalogIndexBuilderService> _logger;

    public ElasticsearchCatalogIndexBuilderService(
        ILogger<ElasticsearchCatalogIndexBuilderService> logger,
        ICatalogIndexService catalogIndexService,
        IIndexDataReader reader,
        IDatasetModelProcessService datasetModelFileProcessService,
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options)
    {
        _logger = logger;
        _catalogIndexService = catalogIndexService;
        _reader = reader;
        _datasetModelFileProcessService = datasetModelFileProcessService;

        // Taken directly rather than reached through ICatalogIndexService: suspending refresh is
        // index administration, and putting it on that port would oblige every implementation of it
        // — including Core's enqueue-only forwarder, which owns no index — to answer for something
        // only a builder ever needs.
        _client = client;
        _index = options.Value.CatalogIndexName;
    }

    public async Task<IndexBuildReport> BuildIndexAsync(CancellationToken cancellationToken = default)
    {
        // Suspended across the whole build, not per resource kind: at the default one-second interval
        // Elasticsearch cuts a fresh segment every second of the rebuild and then has to merge them
        // all. Restored on the way out even if the build throws — see EsRest.SuspendRefreshAsync.
        await using var refreshSuspended = await EsRest.SuspendRefreshAsync(_client, _index, cancellationToken);

        // Sequential: the scoped DbContext cannot be used concurrently.
        var structuresAvailable = await IndexDatasets(cancellationToken);
        await IndexAll("data services", _reader.GetDataServicesInBatches(BuildBatchSize, cancellationToken), cancellationToken);
        await IndexAll("public services", _reader.GetPublicServicesInBatches(BuildBatchSize, cancellationToken), cancellationToken);
        await IndexAll("concepts", _reader.GetConceptsInBatches(BuildBatchSize, cancellationToken), cancellationToken);
        await IndexAll("mapping tables", _reader.GetMappingTablesInBatches(BuildBatchSize, cancellationToken), cancellationToken);

        return new IndexBuildReport(structuresAvailable);
    }

    /// <summary>Returns whether the dataset structures container could be listed.</summary>
    private async Task<bool> IndexDatasets(CancellationToken cancellationToken)
    {
        var count = 0;
        var structuresAvailable = true;
        IEnumerable<string> datasetIds;
        try
        {
            datasetIds = await _datasetModelFileProcessService.GetAllDatasetIdsWithStructures(cancellationToken);
        }
        catch (Exception ex)
        {
            // Not fatal — an index without the structure flag beats no index — but it silently
            // mislabels every dataset as structure-less, so the caller is told rather than left to
            // infer it from a Structures facet that looks broken.
            _logger.LogError(
                ex,
                "Could not read dataset structures (object store). Every dataset will be indexed as " +
                "having no structure, so the Structures facet will be empty and the structure filter " +
                "will match nothing. Check ObjectStoreConfiguration.");
            datasetIds = [];
            structuresAvailable = false;
        }

        var structureIds = datasetIds.ToArray();

        try
        {
            await foreach (var batch in _reader.GetDatasetsInBatches(BuildBatchSize, cancellationToken))
            {
                // The reader leaves HasStructure null because the flag lives in the object store.
                // A rebuild knows the answer for every dataset, so resolve it here rather than
                // letting the engine read each one back out of the index.
                var entries = batch
                    .Select(x => x with { HasStructure = structureIds.Any(id => id.StartsWith(x.Id.ToString())) })
                    .ToArray();

                await _catalogIndexService.UpdateIndexAsync(entries, cancellationToken);
                count += entries.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A problem occurred while indexing datasets into Elasticsearch.");
        }

        _logger.LogInformation("{Count} datasets indexed into Elasticsearch.", count);

        return structuresAvailable;
    }

    private async Task IndexAll(
        string what,
        IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> batches,
        CancellationToken cancellationToken)
    {
        var count = 0;

        try
        {
            await foreach (var batch in batches)
            {
                await _catalogIndexService.UpdateIndexAsync(batch, cancellationToken);
                count += batch.Count;
            }
        }
        catch (Exception ex)
        {
            // One resource kind failing must not abandon the rest of the rebuild: a partial index
            // beats none, and the log says which part is missing.
            _logger.LogError(ex, "A problem occurred while indexing {What} into Elasticsearch.", what);
        }

        _logger.LogInformation("{Count} {What} indexed into Elasticsearch.", count, what);
    }
}
