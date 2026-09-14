using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogSearchEngine : ICatalogSearchEngine
{
    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;
    private readonly ILogger<ElasticsearchCatalogSearchEngine> _logger;

    public ElasticsearchCatalogSearchEngine(
        ElasticsearchSearchExecutor executor,
        IndexNames names,
        ILogger<ElasticsearchCatalogSearchEngine> logger)
    {
        _executor = executor;
        _names = names;
        _logger = logger;
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

        var result = CatalogResponseReader.ReadSearch(response.RootElement, page, pageSize, out var skipped);

        if (skipped > 0)
        {
            _logger.LogWarning(
                "Dropped {Skipped} of {Returned} hits from {Index}: their documents carry no readable "
                + "id. TotalCount still counts them.",
                skipped,
                skipped + result.Results.Count,
                _names.Catalog);
        }

        return result;
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
