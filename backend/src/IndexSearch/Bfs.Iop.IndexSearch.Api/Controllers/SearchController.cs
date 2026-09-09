using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
///     Reading the index. Every response is served from Elasticsearch; code list ancestors are
///     written into the document at index time, so no request reaches the database.
/// </summary>
[ApiController]
[Route("api/search")]
[Produces("application/json")]
public sealed class SearchController : ControllerBase
{
    private readonly ICatalogSearchEngine _catalog;
    private readonly ICodeListSearchEngine _codeLists;
    private readonly ISearchCallerFactory _callers;
    private readonly IndexSearchOptions _options;

    public SearchController(
        ICatalogSearchEngine catalog,
        ICodeListSearchEngine codeLists,
        ISearchCallerFactory callers,
        IOptions<IndexSearchOptions> options)
    {
        _catalog = catalog;
        _codeLists = codeLists;
        _callers = callers;
        _options = options.Value;
    }

    /// <summary>Searches the catalog.</summary>
    /// <param name="query">Free text. Empty matches everything.</param>
    /// <param name="language">de, en, fr, it or rm. Defaults to de; anything else falls back to it.</param>
    /// <param name="page">One-based. A page past the searchable window comes back empty.</param>
    /// <param name="pageSize">Clamped to IndexSearch:MaxPageSize.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    [HttpGet("catalog")]
    public Task<PagedResult<CatalogSearchHit>> SearchCatalog(
        [FromQuery] string? query,
        [FromQuery] string? language,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        _catalog.SearchAsync(
            query,
            Languages(language),
            filter: null,
            Caller(),
            page,
            PageSize(pageSize),
            cancellationToken);

    /// <summary>
    ///     Counts the catalog by facet. A separate call from the search because the counts are
    ///     drill-sideways: each dimension is counted with every selection except its own applied.
    /// </summary>
    /// <param name="query">Free text. Empty counts everything.</param>
    /// <param name="language">de, en, fr, it or rm. Defaults to de; anything else falls back to it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    [HttpGet("catalog/facets")]
    public Task<CatalogFacetCounts> CountCatalog(
        [FromQuery] string? query,
        [FromQuery] string? language,
        CancellationToken cancellationToken = default) =>
        _catalog.CountAsync(query, Languages(language), filter: null, Caller(), cancellationToken);

    [HttpGet("codelists/{conceptId:guid}")]
    public Task<PagedResult<CodeListSearchHit>> SearchCodeList(
        Guid conceptId,
        [FromQuery] string? query,
        [FromQuery] string? language,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        _codeLists.SearchAsync(
            conceptId,
            query,
            Language(language),
            filter: null,
            page,
            PageSize(pageSize),
            cancellationToken);

    private int PageSize(int pageSize) => Math.Clamp(pageSize, 1, _options.MaxPageSize);

    internal static string Language(string? language) =>
        IndexLanguages.All.FirstOrDefault(
            x => string.Equals(x, language?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? IndexLanguages.Default;

    internal static IReadOnlyList<string> Languages(string? language) => [Language(language)];

    private SearchCaller Caller() => _callers.Create();
}
