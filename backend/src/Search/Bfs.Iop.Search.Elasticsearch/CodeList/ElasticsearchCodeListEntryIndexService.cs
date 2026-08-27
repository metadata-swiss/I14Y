using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Indexing;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Search.Abstractions;

using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Search.Elasticsearch.CodeList;

/// <summary>
/// Elasticsearch-backed <see cref="ICodeListEntryIndexService"/>, used by the core
/// <c>IopConceptsService</c> for live CRUD indexing.
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

    public async Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default) =>
        await EsRest.EnsureIndexAsync(_client, _index, CodeListIndexMapping.BuildCreateIndexJson(), recreate, cancellationToken);

    public async Task BuildIndexAsync(CancellationToken cancellationToken = default)
    {
        var reader = _serviceProvider.GetRequiredService<IIndexDataReader>();

        _logger.LogInformation("Start building Elasticsearch CodeListEntry index.");

        var count = 0;
        await foreach (var batch in reader.GetCodeListEntriesInBatches(100, cancellationToken))
        {
            try
            {
                await BulkIndexAsync(batch);
                count += batch.Count;
            }
            catch (Exception ex)
            {
                // A bad batch is logged and skipped so the rest of the codelist index still builds.
                _logger.LogError(ex, "A batch of code-list entries could not be indexed into Elasticsearch.");
            }
        }

        _logger.LogInformation("{Count} code-list entries indexed into Elasticsearch.", count);
    }

    public Task IndexAsync(IEnumerable<CodeListIndexEntry> codeListEntries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries);
        return BulkIndexAsync(codeListEntries);
    }

    // Bulk "index" upserts by _id, so a separate delete is unnecessary.
    public Task UpdateIndexAsync(IEnumerable<CodeListIndexEntry> codeListEntries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries);
        return BulkIndexAsync(codeListEntries);
    }

    public Task DeIndexAsync(IEnumerable<Guid> codeListEntriesIds, CancellationToken cancellationToken = default)
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

        return lines.Count > 0 ? SendBulkAsync(lines) : Task.CompletedTask;
    }

    private Task BulkIndexAsync(IEnumerable<CodeListIndexEntry> entries)
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

        return lines.Count > 0 ? SendBulkAsync(lines) : Task.CompletedTask;
    }

    private async Task SendBulkAsync(List<object> lines)
    {
        var response = await EsRest.BulkAsync(_client, lines);
        // Fail loudly on write errors; BuildIndex wraps batches so a partial failure
        // during a full rebuild is logged and skipped rather than aborting the whole codelist index.
        var body = EsRest.ReadBodyOrThrow(response, "codelist bulk");
        if (EsRest.HasBulkErrors(body))
        {
            _logger.LogError("Elasticsearch codelist bulk request reported item errors: {Body}", body);
            throw new InvalidOperationException("Elasticsearch codelist bulk request reported item errors. See logs for details.");
        }
    }
}
