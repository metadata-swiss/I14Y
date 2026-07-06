using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Partner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class SearchController : ControllerBase
{
    private readonly IIopCoreApiClient _apiClient;

    public SearchController(IIopCoreApiClient apiClient) =>
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

    /// <summary>
    /// Returns the search results for the given filters.
    /// </summary>
    /// <param name="language">Language to use for the search.</param>
    /// <param name="query">Search query.</param>
    /// <param name="accessRights">Access rights to search.</param>
    /// <param name="businessEvents">Business events to search.</param>
    /// <param name="conceptValueTypes">Concept types to search (example: Numeric). Only applicable for concepts.</param>
    /// <param name="formats">Formats to search. Only applicatble datasets with distributions containing the specified formats.</param>
    /// <param name="publicationLevels">Publication levels to search.</param>
    /// <param name="publicationLevelProposals">Publication level proposals to search.</param>
    /// <param name="lifeEvents">Life events to search. Only applicable to public services.</param>
    /// <param name="publishers">Publisher identifiers to search.</param>
    /// <param name="registrationStatuses">Registration status to search.</param>
    /// <param name="registrationStatusProposals">Registration status proposals to search.</param>
    /// <param name="structure">Structure value to search. Only applicable to datasets.</param>
    /// <param name="themes">Themes to search.</param>
    /// <param name="types">Catalog types to search (example: Dataset).</param>
    /// <param name="page" example="1">Page number.</param>
    /// <param name="pageSize" example="25">Max number of results per page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [BadRequest]
    [Ok(typeof(DataWrapper<ICollection<SearchResultModel>>))]
    public async Task<DataWrapper<ICollection<SearchResultModel>>> Search(
        [FromQuery] Language? language,
        [FromQuery] string? query,
        [FromQuery] string[] accessRights,
        [FromQuery] string[] businessEvents,
        [FromQuery] ConceptType[] conceptValueTypes,
        [FromQuery] string[] formats,
        [FromQuery] PublicationLevel[] publicationLevels,
        [FromQuery] PublicationLevel[] publicationLevelProposals,
        [FromQuery] string[] lifeEvents,
        [FromQuery] string[] publishers,
        [FromQuery] RegistrationStatus[] registrationStatuses,
        [FromQuery] RegistrationStatus[] registrationStatusProposals, 
        [FromQuery] SearchStructureOption? structure,
        [FromQuery] string[] themes,
        [FromQuery] SearchResourceType[] types,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetSearchByLanguageAndQueryAndAccessRightsAndBusinessEventsAndConceptValueTypesAndFormatsAndLevelsAndLevelProposalsAndLifeEventsAndPublishersAndStatusesAndStatusProposalsAndStructureAndThemesAndTypesAndPageAndPageSizeAsync(
            language?.ToString(),
            query,
            accessRights,
            businessEvents,
            conceptValueTypes,
            formats,
            publicationLevels,
            publicationLevelProposals,
            lifeEvents,
            publishers,
            registrationStatuses,
            registrationStatusProposals,
            structure,
            themes,
            types,
            page,
            pageSize,
            cancellationToken);

        var pageHeaderValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageHeaderKey);
        var pageSizeValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.PageSizeHeaderKey);
        var totalPagesValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalPagesHeaderKey);
        var totalRowsValue = response.TryGetSwaggerHeaderValue(HttpContextExtensions.TotalRowsHeaderKey);

        HttpContext.Response.Headers.Append(HttpContextExtensions.PageHeaderKey, pageHeaderValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.PageSizeHeaderKey, pageSizeValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalPagesHeaderKey, totalPagesValue);
        HttpContext.Response.Headers.Append(HttpContextExtensions.TotalRowsHeaderKey, totalRowsValue);

        return response.Result.Wrap();
    }
}
