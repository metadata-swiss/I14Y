using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
/// Catalog search. Route and query-parameter names match IOP Core's /api/Search exactly, so a client
/// migrates by changing its base URL only.
/// <para>
/// Anonymous, but the results are still user-scoped: the query builder applies a publication-level
/// and agency filter derived from the caller's token. An unauthenticated caller therefore sees only
/// public resources — which is also what happens if JWT validation is misconfigured, so a wrong
/// Keycloak/Eiam setting shows up as "results went missing", not as an error.
/// </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class SearchController : ControllerBase
{
    private readonly ICatalogSearchQueryService _searchService;

    public SearchController(ICatalogSearchQueryService searchService) => _searchService = searchService;

    /// <summary>Searches the catalog.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SearchResultModel>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<SearchResultModel>> Search(
        [FromQuery] string? language,
        [FromQuery] string? query,
        [FromQuery] string[] accessRights,
        [FromQuery] string[] businessEvents,
        [FromQuery] ConceptType[] conceptValueTypes,
        [FromQuery] string[] formats,
        [FromQuery] PublicationLevel[] levels,
        [FromQuery] PublicationLevel[] levelProposals,
        [FromQuery] string[] lifeEvents,
        [FromQuery] string[] publishers,
        [FromQuery] RegistrationStatus[] statuses,
        [FromQuery] RegistrationStatus[] statusProposals,
        [FromQuery] SearchStructureOption? structure,
        [FromQuery] string[] themes,
        [FromQuery] SearchResourceType[] types,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(
            accessRights, businessEvents, conceptValueTypes, formats, levels, levelProposals,
            lifeEvents, publishers, statuses, statusProposals, structure, themes, types);

        // Same defaulting as IOP Core: no paging means "everything", capped by the engine.
        var (resolvedPage, resolvedPageSize) = page.HasValue && pageSize.HasValue
            ? (page.Value, pageSize.Value)
            : (1, int.MaxValue);

        var results = await _searchService.SearchAsync(query, language, filter, resolvedPage, resolvedPageSize, cancellationToken);

        HttpContext.AddPagingHeaders(results.Page, results.PageSize, results.TotalCount);

        return results.Results;
    }

    internal static CatalogSearchFilter BuildFilter(
        string[] accessRights,
        string[] businessEvents,
        ConceptType[] conceptValueTypes,
        string[] formats,
        PublicationLevel[] levels,
        PublicationLevel[] levelProposals,
        string[] lifeEvents,
        string[] publishers,
        RegistrationStatus[] statuses,
        RegistrationStatus[] statusProposals,
        SearchStructureOption? structure,
        string[] themes,
        SearchResourceType[] types) => new()
        {
            AccessRights = accessRights,
            BusinessEvents = businessEvents,
            ConceptValueTypes = conceptValueTypes,
            Formats = formats,
            Structure = structure,
            LifeEvents = lifeEvents,
            PublicationLevelProposals = levelProposals,
            PublicationLevels = levels,
            PublisherIdentifiers = publishers,
            RegistrationStatuses = statuses,
            RegistrationStatusProposals = statusProposals,
            Themes = themes,
            Types = types,
        };
}
