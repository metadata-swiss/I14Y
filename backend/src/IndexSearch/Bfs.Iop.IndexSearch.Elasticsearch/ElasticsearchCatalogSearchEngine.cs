using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogSearchEngine : ICatalogSearchEngine
{
    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;

    public ElasticsearchCatalogSearchEngine(ElasticsearchSearchExecutor executor, IndexNames names)
    {
        _executor = executor;
        _names = names;
    }

    public async Task<PagedResult<CatalogSearchHit>> SearchAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (from, size) = Paging.ToWindow(page, pageSize);

        var body = CatalogQueryBuilder.BuildSearchBody(query, languages, filter, caller, from, size);

        using var response = await _executor.SearchAsync(_names.Catalog, body, cancellationToken);

        return CatalogResponseReader.ReadSearch(response.RootElement, page, pageSize);
    }

    public async Task<CatalogFacetCounts> CountAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CancellationToken cancellationToken = default)
    {
        var body = CatalogQueryBuilder.BuildCountBody(query, languages, filter, caller);

        using var response = await _executor.SearchAsync(_names.Catalog, body, cancellationToken);

        return CatalogResponseReader.ReadFacets(response.RootElement);
    }
}
