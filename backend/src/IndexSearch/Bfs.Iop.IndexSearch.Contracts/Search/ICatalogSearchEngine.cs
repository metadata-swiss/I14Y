using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts;

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
        CatalogSearchSort sort = CatalogSearchSort.Relevance,
        string sortLanguage = IndexLanguages.Default,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<CatalogSearchHit> StreamAllAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CatalogSearchSort sort = CatalogSearchSort.Relevance,
        string sortLanguage = IndexLanguages.Default,
        CancellationToken cancellationToken = default);

    Task<CatalogFacetCounts> CountAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CancellationToken cancellationToken = default);
}