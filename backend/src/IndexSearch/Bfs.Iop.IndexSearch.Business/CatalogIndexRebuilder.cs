using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Business;


public sealed record IndexRebuildReport(
    int DocumentsSent,
    int DocumentsWritten,
    int BatchesFailed,
    bool? StructuresResolved = null);

public sealed class CatalogIndexRebuilder
{
    private readonly ICatalogDocumentSource _source;
    private readonly IDatasetStructureSource _structures;
    private readonly ICatalogIndexWriter _writer;
    private readonly ILogger<CatalogIndexRebuilder> _logger;

    public CatalogIndexRebuilder(
        ICatalogDocumentSource source,
        IDatasetStructureSource structures,
        ICatalogIndexWriter writer,
        ILogger<CatalogIndexRebuilder> logger)
    {
        _source = source;
        _structures = structures;
        _writer = writer;
        _logger = logger;
    }

    public async Task<IndexRebuildReport> RebuildAsync(int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var structures = await _structures.GetIdsWithStructuresAsync(cancellationToken);

        if (structures is null)
        {
            _logger.LogError(
                "Could not read dataset structures. Datasets will be indexed without changing their "
                + "structure flag, so the Structures facet will reflect whatever was there before.");
        }

        var sent = 0;
        var written = 0;
        var failed = 0;

        await foreach (var batch in _source.ReadAllAsync(batchSize, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var documents = structures is null
                ? batch
                : [.. batch.Select(x => WithStructureFlag(x, structures))];

            sent += documents.Count;

            try
            {
                written += await _writer.WriteAsync(documents, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {

                failed++;
                _logger.LogError(ex, "A batch of {Count} catalog documents could not be indexed.", documents.Count);
            }
        }

        return new IndexRebuildReport(sent, written, failed, structures is not null);
    }


    private static CatalogIndexDocument WithStructureFlag(CatalogIndexDocument document, IReadOnlySet<Guid> structures) =>
        document.Type == SearchResourceType.Dataset
            ? document with { HasStructure = structures.Contains(document.Id) }
            : document;
}
