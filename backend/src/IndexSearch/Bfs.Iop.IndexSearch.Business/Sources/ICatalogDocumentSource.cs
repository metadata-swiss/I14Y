using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface ICatalogDocumentSource
{
    IAsyncEnumerable<IReadOnlyList<CatalogIndexDocument>> ReadAllAsync(
        int batchSize,
        CancellationToken cancellationToken = default);
}
