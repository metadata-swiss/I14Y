using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns the search results for the given filters.
    /// </summary>
    /// <param name="language">The language to use for the search</param>
    /// <param name="query">The search query</param>
    /// <param name="accessRights">Only results with one of the specified access rights (PUBLIC, NON_PUBLIC, RESTRICTED) are returned</param>
    /// <param name="businessEvents">Only results corresponding to one of the specified business events are returned</param>
    /// <param name="conceptValueTypes">Only results corresponding to one of the specified IOP concept value types (CodeList, Date, Numeric, String) are returned.</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are returned</param>
    /// <param name="structure">Only results for datasets are returned.</param>
    /// <param name="levels">Only results with one of the specified publication levels (Unit, I14Y Public) are returned</param>
    /// <param name="levelProposals">Only results with the proposal publication (I14Y Public) is returned</param>
    /// <param name="lifeEvents">Only results corresponding to one of the specified life events are returned</param>
    /// <param name="publishers">Only results with one of the specified publishers are returned</param>
    /// <param name="statuses">Only results with one of the specified registration statuses are returned</param>
    /// <param name="statusProposals">Only results with one of the proposals registration statuses are returned</param>
    /// <param name="themes">Only results corresponding to one of the specified themes are returned</param>
    /// <param name="types">Only results with one of the specified types are returned</param>
    /// <param name="page" example="1">The number of the result page to return</param>
    /// <param name="pageSize" example="25">The size of each result page</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesJson]
    [Ok(typeof(IEnumerable<SearchResultModel>))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<IEnumerable<SearchResultModel>> Search([FromQuery] string? language,
    [FromQuery] string? query, [FromQuery] string[] accessRights, [FromQuery] string[] businessEvents, [FromQuery] ConceptType[] conceptValueTypes,
    [FromQuery] string[] formats, [FromQuery] PublicationLevel[] levels, [FromQuery] PublicationLevel[] levelProposals,
    [FromQuery] string[] lifeEvents, [FromQuery] string[] publishers, [FromQuery] RegistrationStatus[] statuses,
    [FromQuery] RegistrationStatus[] statusProposals, [FromQuery] SearchStructureOption? structure, 
    [FromQuery] string[] themes, [FromQuery] SearchResourceType[] types,
    [FromQuery] int? page, [FromQuery] int? pageSize, 
    CancellationToken cancellationToken = default)
    {
        var command = new GetCatalogSearchCommand(
            query,
            language,
            new()
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
            },
            page,
            pageSize);

        var response = await _mediator.Send(command, cancellationToken);
        HttpContext.AddPagingHeaders(response.Page, response.PageSize, response.TotalCount);
        return response.Results;
    }

    /// <summary>
    /// Get the counter for each possible filter values corresponding to a search query
    /// </summary>
    /// <param name="language">The language to use for the search</param>
    /// <param name="query">The search query</param>
    /// <param name="accessRights">Only results with one of the specified access rights (PUBLIC, NON_PUBLIC, RESTRICTED) are counted</param>
    /// <param name="businessEvents">Only results corresponding to one of the specified business events are returned</param>
    /// <param name="conceptValueTypes">Only results corresponding to one of the specified IOP concept value types (CodeList, Date, Numeric, String) are counted.</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are counted</param>
    /// <param name="levels">Only results with one of the specified publication levels (Unit, I14Y Public) are counted</param>
    /// <param name="levelProposals">Only results with the proposal publication (I14Y Public) is returned </param>
    /// <param name="lifeEvents">Only results corresponding to one of the specified life events are returned</param>
    /// <param name="publishers">Only results with one of the specified publishers are counted</param>
    /// <param name="statuses">Only results with one of the specified registration statuses are counted</param>
    /// <param name="statusProposals">Only results with one of the proposals registration statuses are returned </param>
    /// <param name="structure">Only results for datasets are returned.</param>
    /// <param name="themes">Only results corresponding to one of the specified themes are counted</param>
    /// <param name="types">Only results corresponding to one of the specified types are counted</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("count")]
    [ProducesJson]
    [Ok(typeof(SearchCountResultModel))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<SearchCountResultModel> SearchCount([FromQuery] string? language, [FromQuery] string? query,
        [FromQuery] string[] accessRights, [FromQuery] string[] businessEvents, [FromQuery] ConceptType[] conceptValueTypes, [FromQuery] string[] formats,
        [FromQuery] PublicationLevel[] levels, [FromQuery] PublicationLevel[] levelProposals, [FromQuery] string[] lifeEvents,
        [FromQuery] string[] publishers, [FromQuery] RegistrationStatus[] statuses, [FromQuery] RegistrationStatus[] statusProposals,
        [FromQuery] SearchStructureOption? structure, [FromQuery] string[] themes, 
        [FromQuery] SearchResourceType[] types, CancellationToken cancellationToken)
    {
        var command = new GetCatalogSearchCountCommand(query, language, new()
        {
            AccessRights = accessRights,
            BusinessEvents = businessEvents,
            ConceptValueTypes = conceptValueTypes,
            Formats = formats,
            LifeEvents = lifeEvents,
            PublicationLevelProposals = levelProposals,
            PublicationLevels = levels,
            PublisherIdentifiers = publishers,
            RegistrationStatuses = statuses,
            RegistrationStatusProposals = statusProposals,
            Structure = structure,
            Themes = themes,
            Types = types,
        });

        return await _mediator.Send(command, cancellationToken);
    }

    /// <summary>
    /// Returns the number of incoming references ("relations" count) for each of the given
    /// catalogue resources, together with a per-type breakdown. Intended to be called
    /// asynchronously by the catalogue page after the search results have been rendered.
    /// </summary>
    /// <param name="items">The catalogue resources (id + type) to count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One <see cref="RelationsCountModel"/> per requested resource.</returns>
    [HttpPost]
    [Route("relations-count")]
    [ProducesJson]
    [Ok(typeof(IReadOnlyList<RelationsCountModel>))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<IReadOnlyList<RelationsCountModel>> RelationsCount(
        [FromBody] IReadOnlyList<RelationsCountRequestItem> items,
        CancellationToken cancellationToken = default)
    {
        return await _mediator.Send(new GetRelationsCountCommand(items), cancellationToken);
    }
}
