using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Settings;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Elasticsearch.CodeList;

/// <summary>
/// Elasticsearch-backed <see cref="ICodeListEntryIndexService"/>. The interface exposes a Lucene
/// <c>Directory</c> (<see cref="IndexDirectory"/>) that only the Lucene search path uses — it throws
/// here. The interface must still be implemented because the core <c>IopConceptsService</c> depends on
/// it directly for live CRUD indexing.
/// </summary>
internal sealed class ElasticsearchCodeListEntryIndexService : ICodeListEntryIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ElasticsearchCodeListEntryIndexService> _logger;
    private readonly string _index;

    public ElasticsearchCodeListEntryIndexService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        IServiceProvider serviceProvider,
        ILogger<ElasticsearchCodeListEntryIndexService> logger)
    {
        _client = client;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _index = options.Value.CodeListIndexName;
    }

    public global::Lucene.Net.Store.Directory IndexDirectory =>
        throw new NotSupportedException("IndexDirectory is Lucene-specific and unused under the Elasticsearch engine.");

    public async Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default) =>
        await EsRest.EnsureIndexAsync(_client, _index, CodeListIndexMapping.BuildCreateIndexJson(), recreate, cancellationToken);

    public async Task BuildIndex(CancellationToken cancellationToken = default)
    {
        // Resolve lazily to avoid a circular dependency with IopConceptsService (mirrors the Lucene service).
        var conceptsService = _serviceProvider.GetRequiredService<IIopConceptsService>();

        _logger.LogInformation("Start building Elasticsearch CodeListEntry index.");

        var count = 0;
        await foreach (var batch in conceptsService.GetCodeListEntriesForIndexInBatches(100, cancellationToken))
        {
            BulkIndex(batch);
            count += batch.Count;
        }

        _logger.LogInformation("{Count} code-list entries indexed into Elasticsearch.", count);
    }

    public void Index(IEnumerable<CodeListEntryModel> codeListEntries)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries);
        BulkIndex(codeListEntries);
    }

    // Bulk "index" upserts by _id, so a separate delete is unnecessary.
    public void UpdateIndex(IEnumerable<CodeListEntryModel> codeListEntries)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries);
        BulkIndex(codeListEntries);
    }

    public void DeIndex(IEnumerable<Guid> codeListEntriesIds)
    {
        ArgumentNullException.ThrowIfNull(codeListEntriesIds);

        var lines = new List<object>();
        foreach (var id in codeListEntriesIds)
        {
            lines.Add(new Dictionary<string, object?>
            {
                ["delete"] = new Dictionary<string, object?> { ["_index"] = _index, ["_id"] = id.ToString() },
            });
        }

        if (lines.Count > 0)
        {
            SendBulk(lines);
        }
    }

    private void BulkIndex(IEnumerable<CodeListEntryModel> entries)
    {
        var lines = new List<object>();
        foreach (var entry in entries)
        {
            var (id, doc) = CodeListDocumentFactory.Build(entry);
            lines.Add(new Dictionary<string, object?>
            {
                ["index"] = new Dictionary<string, object?> { ["_index"] = _index, ["_id"] = id },
            });
            lines.Add(doc);
        }

        if (lines.Count > 0)
        {
            SendBulk(lines);
        }
    }

    private void SendBulk(List<object> lines)
    {
        var response = EsRest.BulkAsync(_client, lines).GetAwaiter().GetResult();
        if (response.ApiCallDetails?.HasSuccessfulStatusCode != true || EsRest.HasBulkErrors(response.Body))
        {
            _logger.LogError("Elasticsearch codelist bulk request failed or reported item errors: {Body}", response.Body);
        }
    }
}
