using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public interface ICatalogSearchEngine
{
    Task<PagedResult<CatalogSearchHit>> SearchAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CatalogFacetCounts> CountAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CancellationToken cancellationToken = default);
}