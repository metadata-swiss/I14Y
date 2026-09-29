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
    /// <summary>
    ///     The largest page this API will serve. Elasticsearch would go to 10 000, which is a lot of
    ///     rows to hand an anonymous caller for one request; nothing in the product pages beyond a
    ///     few hundred, and a caller wanting everything should page.
    /// </summary>
    internal const int MaxPageSize = 200;

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
    /// <param name="pageSize">Bounded to 200; a larger value is clamped rather than refused.</param>
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
            Bounded(pageSize),

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
            Bounded(pageSize),

            cancellationToken);

    /// <summary>
    ///     Searches the catalog, taking the filter in the body. The GET above cannot: the filter has
    ///     fourteen dimensions, each a list, and a caller selecting freely overruns what a proxy will
    ///     accept on a query string.
    /// </summary>
    [HttpPost("catalog")]
    public Task<PagedResult<CatalogSearchHit>> SearchCatalog(
        [FromBody] CatalogSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _catalog.SearchAsync(
            request.Query,
            Languages(request.Language),
            request.Filter,
            Caller(),
            request.Page,
            Bounded(request.PageSize),

            cancellationToken);
    }

    /// <summary>Counts the catalog by facet, taking the filter in the body.</summary>
    [HttpPost("catalog/facets")]
    public Task<CatalogFacetCounts> CountCatalog(
        [FromBody] CatalogFacetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _catalog.CountAsync(
            request.Query,
            Languages(request.Language),
            request.Filter,
            Caller(),
            cancellationToken);
    }

    /// <summary>Searches one concept's code list entries, taking the filter in the body.</summary>
    [HttpPost("codelists")]
    public Task<PagedResult<CodeListSearchHit>> SearchCodeList(
        [FromBody] CodeListSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _codeLists.SearchAsync(
            request.ConceptId,
            request.Query,
            Language(request.Language),
            request.Filter,
            request.Page,
            Bounded(request.PageSize),

            cancellationToken);
    }

    internal static string Language(string? language) =>
        IndexLanguages.All.FirstOrDefault(
            x => string.Equals(x, language?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? IndexLanguages.Default;

    internal static IReadOnlyList<string> Languages(string? language) => IndexLanguages.All;

    /// <summary>
    ///     Every matching code list entry, for an export.
    ///     <para>
    ///         Deliberately unpaged: the caller wants the whole set, and walking it by page number
    ///         would stop at the index result window while the largest code lists hold more than twice
    ///         that. The engine walks a frozen point-in-time view instead.
    ///     </para>
    /// </summary>
    [HttpPost("codelists/all")]
    public async Task<IReadOnlyList<CodeListSearchHit>> SearchAllCodeListEntries(
        [FromBody] CodeListSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var all = new List<CodeListSearchHit>();

        await foreach (var hit in _codeLists.StreamAllAsync(
            request.ConceptId,
            request.Query,
            Language(request.Language),
            request.Filter,
            cancellationToken))
        {
            all.Add(hit);
        }

        return all;
    }

    /// <summary>The page size a route will actually ask the engine for.</summary>
    internal static int Bounded(int pageSize) => Math.Clamp(pageSize, 0, MaxPageSize);

    private SearchCaller Caller() => _callers.Create();
}
