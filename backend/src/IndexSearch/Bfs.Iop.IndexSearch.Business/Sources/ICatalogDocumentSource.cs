using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Business.Sources;

public interface ICatalogDocumentSource
{
    IAsyncEnumerable<IReadOnlyList<CatalogIndexDocument>> ReadAllAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     One resource, for keeping the index current between rebuilds. 
    /// </summary>
    Task<CatalogIndexDocument?> ReadOneAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default);
}
