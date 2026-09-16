using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface ICodeListDocumentSource
{
    IAsyncEnumerable<IReadOnlyList<CodeListIndexDocument>> ReadAllAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}