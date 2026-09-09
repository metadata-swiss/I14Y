namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public interface ICatalogIndexWriter
{
    Task<int> WriteAsync(
        IReadOnlyCollection<CatalogIndexDocument> documents,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}