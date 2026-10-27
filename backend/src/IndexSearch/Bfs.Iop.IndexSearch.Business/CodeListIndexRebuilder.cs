using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Business;

public sealed class CodeListIndexRebuilder
{
    private readonly ICodeListDocumentSource _source;
    private readonly ICodeListIndexWriter _writer;
    private readonly ILogger<CodeListIndexRebuilder> _logger;

    public CodeListIndexRebuilder(
        ICodeListDocumentSource source,
        ICodeListIndexWriter writer,
        ILogger<CodeListIndexRebuilder> logger)
    {
        _source = source;
        _writer = writer;
        _logger = logger;
    }

    public async Task<IndexRebuildReport> RebuildAsync(int batchSize = 1000, CancellationToken cancellationToken = default)
    {
        var sent = 0;
        var written = 0;
        var failed = 0;

        await foreach (var batch in _source.ReadAllAsync(batchSize, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            sent += batch.Count;

            try
            {
                written += await _writer.WriteAsync(batch, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failed++;
                _logger.LogError(ex, "A batch of {Count} code list documents could not be indexed.", batch.Count);
            }
        }

        return new IndexRebuildReport(sent, written, failed);
    }
}
