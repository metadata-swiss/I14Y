using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Microsoft.AspNetCore.Mvc;

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

    public SearchController(
        ICatalogSearchEngine catalog,
        ICodeListSearchEngine codeLists,
        ISearchCallerFactory callers)
    {
        _catalog = catalog;
        _codeLists = codeLists;
        _callers = callers;
    }

    /// <summary>Searches the catalog.</summary>
    /// <param name="query">Free text. Empty matches everything.</param>
    /// <param name="language">de, en, fr, it or rm. Defaults to de; anything else falls back to it.</param>
    /// <param name="page">One-based. A page past the searchable window comes back empty.</param>
    /// <param name="pageSize">Bounded by what Elasticsearch will serve: from + size at most 10 000.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    [HttpGet("catalog")]
    public Task<PagedResult<CatalogSearchHit>> SearchCatalog(
        [FromQuery] string? query,
        [FromQuery] string? language,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken = default) =>
        _catalog.SearchAsync(
            query,
            Languages(language),
            filter: null,
            Caller(),
            page,
            pageSize,
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
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken = default) =>
        _codeLists.SearchAsync(
            conceptId,
            query,
            Language(language),
            filter: null,
            page,
            pageSize,
            cancellationToken);

    internal static string Language(string? language) =>
        IndexLanguages.All.FirstOrDefault(
            x => string.Equals(x, language?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? IndexLanguages.Default;

    internal static IReadOnlyList<string> Languages(string? language) => [Language(language)];

    private SearchCaller Caller() => _callers.Create();
}
