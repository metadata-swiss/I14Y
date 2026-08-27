using System.ComponentModel.DataAnnotations;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Search.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
/// Index-backed search within a single code list.
/// <para>
/// Only the search route lives here. Everything else IOP Core exposes under
/// <c>/api/Concepts/{id}/codelist-entries</c> reads straight from Postgres and stays in Core; this
/// service owns the index, not the concept.
/// </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ConceptsController : ControllerBase
{
    private readonly ICodeListEntrySearchService _searchService;

    public ConceptsController(ICodeListEntrySearchService searchService) => _searchService = searchService;

    /// <summary>
    /// Searches the entries of one code list. Route and query parameters mirror IOP Core exactly, so
    /// a client migrates by changing its base URL only.
    /// </summary>
    /// <param name="id">Concept id of the code list to search within.</param>
    /// <param name="language">Language whose labels are searched and returned.</param>
    /// <param name="query">Free-text query; null or empty matches every entry.</param>
    /// <param name="filters">Additional field filters applied on top of the query.</param>
    /// <param name="addCodeListEntriesPaths">Whether to resolve each hit's ancestor path.</param>
    /// <param name="page">Page number; omit together with pageSize to return everything.</param>
    /// <param name="pageSize">Page size; omit together with page to return everything.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("{id:guid}/codelist-entries/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CodeListEntrySearchResultEntryModel>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<CodeListEntrySearchResultEntryModel>> SearchCodeListEntries(
        Guid id,
        [Required][FromQuery] string language,
        [FromQuery] string? query,
        [FromQuery] List<string> filters,
        [FromQuery] bool addCodeListEntriesPaths,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        // Same defaulting as IOP Core: paging is all-or-nothing, and omitting it means "everything".
        var (resolvedPage, resolvedPageSize) = page.HasValue && pageSize.HasValue
            ? (page.Value, pageSize.Value)
            : (1, int.MaxValue);

        var pagedResult = await _searchService.SearchAsync(
            id,
            language,
            query,
            filters ?? [],
            addCodeListEntriesPaths,
            resolvedPage,
            resolvedPageSize,
            cancellationToken);

        HttpContext.AddPagingHeaders(pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);

        return pagedResult.Results;
    }
}
