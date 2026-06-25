using Bfs.Iop.Admin.Commands.Catalogs;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Api.Attributes;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Controllers;

/// <summary>
/// The catalog controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    /// <summary>
    /// Initializes a <see cref="CatalogController"/> instance
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="apiClient"></param>
    public CatalogController(IMediator mediator, IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _apiClient = apiClient;
    }

    /// <summary>
    /// Search the catalog for datasets, data services and public services.
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="accessRights">Only results with one of the specified access rights (PUBLIC, NON_PUBLIC, RESTRICTED) are returned </param>
    /// <param name="conceptValueTypes">Only results with one of the specified concept value types are returned</param>
    /// <param name="businessEvents">Only results with one of the specified business events are returned</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are returned </param>
    /// <param name="structure">Only results for datasets are returned.</param>
    /// <param name="levels">Only results corresponding to one of the specified publication levels are returned </param>
    /// <param name="levelProposals">Only results with the proposal publication (I14Y Public) is returned </param>
    /// <param name="lifeEvents">Only results with one of the specified life events are returned</param>
    /// <param name="publishers">Only results with one of the specified publishers are returned </param>
    /// <param name="statuses">Only results with one of the specified registration statuses are returned </param>
    /// <param name="statusProposals">Only results with one of the proposals registration statuses are returned </param>
    /// <param name="themes">Only results with one of the specified themes are returned </param>
    /// <param name="types">Only results with one of the specified types are returned </param>
    /// <param name="page" example="1">The number of the result page to return</param>
    /// <param name="pageSize" example="25">The size of each result page</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpGet("search")]
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<CatalogEntry>))]
    [BadRequest]
    public async Task<IEnumerable<CatalogEntry>> Search(
        [FromQuery] string? query, [FromQuery] string[] accessRights, [FromQuery] ConceptType[]? conceptValueTypes,
        [FromQuery] string[] formats, [FromQuery] string[]? businessEvents, [FromQuery] PublicationLevel[]? levels,
        [FromQuery] PublicationLevel?[]? levelProposals, [FromQuery] string[]? lifeEvents, [FromQuery] string[]? publishers,
        [FromQuery] RegistrationStatus[]? statuses, [FromQuery] RegistrationStatus?[]? statusProposals,
        [FromQuery] SearchStructureOption? structure, [FromQuery] string[]? themes,
        [FromQuery] SearchResourceType[]? types, [FromQuery] int? page, [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var command = new SearchCommand
        {
            AccessRights = accessRights,
            BusinessEvents = businessEvents ?? [],
            ConceptValueTypes = conceptValueTypes ?? [],
            Formats = formats,
            Structure = structure,
            LifeEvents = lifeEvents ?? [],
            Query = query,
            PublicationLevels = levels ?? [],
            PublicationLevelProposals = levelProposals ?? [],
            RegistrationStatuses = statuses ?? [],
            RegistrationStatusProposals = statusProposals ?? [],
            Publishers = publishers ?? [],
            Themes = themes ?? [],
            Types = types ?? [],
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(command, cancellationToken);
        HttpContext.AddPagingHeaders(result.Page, result.PageSize, result.TotalCount);

        return result.Results;
    }

    /// <summary>
    /// Get the result count by filter value corresponding to a search query
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="accessRights">Only results with one of the specified access rights (PUBLIC, NON_PUBLIC, RESTRICTED) are counted</param>
    /// <param name="conceptValueTypes">Only results with one of the specified concept value types are counted</param>
    /// <param name="businessEvents">Only results with one of the specified business events are counted</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are counted</param>
    /// <param name="levels">Only results corresponding to one of the specified publication levels are counted</param>
    /// <param name="levelProposals">Only results with the proposal publication (I14Y Public) is counted</param>
    /// <param name="lifeEvents">Only results with one of the specified life events are counted</param>
    /// <param name="publishers">Only results with one of the specified publishers are counted</param>
    /// <param name="statuses">Only results with one of the specified registration statuses are counted</param>
    /// <param name="statusProposals">Only results with one of the proposals registration statuses are counted</param>
    /// <param name="structure">Only results for datasets are returned.</param>
    /// <param name="themes">Only results with one of the specified themes are counted</param>
    /// <param name="types">Only results corresponding to one of the specified types are counted</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpGet("searchcount")]
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(FilterCountResult))]
    [BadRequest]
    public async Task<FilterCountResult> SearchCount(
        [FromQuery] string? query, [FromQuery] string[] accessRights, [FromQuery] ConceptType[]? conceptValueTypes,
        [FromQuery] string[]? businessEvents, [FromQuery] string[] formats, [FromQuery] PublicationLevel[]? levels,
        [FromQuery] PublicationLevel?[]? levelProposals, [FromQuery] string[]? lifeEvents, [FromQuery] string[]? publishers,
        [FromQuery] RegistrationStatus[]? statuses, [FromQuery] RegistrationStatus?[]? statusProposals,
        [FromQuery] SearchStructureOption? structure, [FromQuery] string[]? themes,
        [FromQuery] SearchResourceType[]? types, CancellationToken cancellationToken)
    {
        var command = new SearchCountCommand
        {
            AccessRights = accessRights,
            BusinessEvents = businessEvents ?? [],
            ConceptValueTypes = conceptValueTypes ?? [],
            Formats = formats,
            Query = query,
            LifeEvents = lifeEvents ?? [],
            PublicationLevels = levels ?? [],
            PublicationLevelProposals = levelProposals ?? [],
            RegistrationStatuses = statuses ?? [],
            RegistrationStatusProposals = statusProposals ?? [],
            Publishers = publishers ?? [],
            Structure = structure,
            Themes = themes ?? [],
            Types = types ?? []
        };

        return await _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Returns the related-by ("relations") count for each of the given catalogue resources, with a
    /// per-type breakdown. Intended to be called asynchronously by the catalogue page after the
    /// search results render.
    /// </summary>
    /// <param name="items">The catalogue resources (id + type) to count.</param>
    /// <param name="cancellationToken"></param>
    [EnableCors("AllowBIT")]
    [HttpPost("relations-count")]
    [ProducesJson]
    [AllowAnonymous]
    [Ok(typeof(IEnumerable<RelationsCountModel>))]
    [BadRequest]
    public async Task<ICollection<RelationsCountModel>> RelationsCount(
        [FromBody] IReadOnlyList<RelationsCountRequestItem> items,
        CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostSearchRelationsCountByBodyAsync(items, cancellationToken);

        return response.Result;
    }
}