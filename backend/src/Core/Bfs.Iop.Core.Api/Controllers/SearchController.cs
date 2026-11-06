using Bfs.Iop.Common.Api.Attributes;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using Cerise;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using CatalogSearchSort = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchSort;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIndexSearchApiClient _indexSearchApiClient;
    private readonly IUserContextService _userContextService;

    public SearchController(
        IMediator mediator,
        IIndexSearchApiClient indexSearchApiClient,
        IUserContextService userContextService)
    {
        _mediator = mediator;
        _indexSearchApiClient = indexSearchApiClient;
        _userContextService = userContextService;
    }

    /// <summary>
    /// Returns the search results for the given filters.
    /// </summary>
    /// <param name="language">The language to use for the search</param>
    /// <param name="query">The search query</param>
    /// <param name="accessRights">Only results with one of the specified access rights (PUBLIC, NON_PUBLIC, RESTRICTED) are returned</param>
    /// <param name="attributedAgents">Only results with a qualified attribution to one of the specified agents (by identifier) are returned</param>
    /// <param name="businessEvents">Only results corresponding to one of the specified business events are returned</param>
    /// <param name="conceptValueTypes">Only results corresponding to one of the specified IOP concept value types (CodeList, Date, Numeric, String) are returned.</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are returned</param>
    /// <param name="keywords">Only results matching one of the specified keywords are returned</param>
    /// <param name="structure">Only results for datasets are returned.</param>
    /// <param name="levels">Only results with one of the specified publication levels (Unit, I14Y Public) are returned</param>
    /// <param name="levelProposals">Only results with the proposal publication (I14Y Public) is returned</param>
    /// <param name="lifeEvents">Only results corresponding to one of the specified life events are returned</param>
    /// <param name="licenses">Only results with at least one distribution providing one of the specified licenses are returned</param>
    /// <param name="publishers">Only results with one of the specified publishers are returned</param>
    /// <param name="statuses">Only results with one of the specified registration statuses are returned</param>
    /// <param name="statusProposals">Only results with one of the proposals registration statuses are returned</param>
    /// <param name="themes">Only results corresponding to one of the specified themes are returned</param>
    /// <param name="types">Only results with one of the specified types are returned</param>
    /// <param name="sort">The order of the results; relevance when not specified</param>
    /// <param name="page" example="1">The number of the result page to return</param>
    /// <param name="pageSize" example="25">The size of each result page</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesJson]
    [Ok(typeof(IEnumerable<SearchResultModel>))]
    [BadRequest]
    [AllowAnonymous]
    public async Task<IEnumerable<SearchResultModel>> Search(
        [FromQuery] string? language,
        [FromQuery] string? query,
        [FromQuery] string[] accessRights,
        [FromQuery] string[] attributedAgents,
        [FromQuery] string[] businessEvents,
        [FromQuery] ConceptType[] conceptValueTypes,
        [FromQuery] string[] formats,
        [FromQuery] string[] keywords,
        [FromQuery] string[] licenses,
        [FromQuery] PublicationLevel[] levels,
        [FromQuery] PublicationLevel[] levelProposals,
        [FromQuery] string[] lifeEvents,
        [FromQuery] string[] publishers,
        [FromQuery] RegistrationStatus[] statuses,
        [FromQuery] RegistrationStatus[] statusProposals,
        [FromQuery] SearchStructureOption? structure,
        [FromQuery] string[] themes,
        [FromQuery] SearchResourceType[] types,
        [FromQuery] CatalogSearchSort? sort,
        [FromQuery] int? page,
        [FromQuery][Range(1, int.MaxValue)] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var command = new GetCatalogSearchCommand(
            query,
            language,
            new()
            {
                AccessRights = accessRights,
                AttributedAgentIdentifiers = attributedAgents,
                BusinessEvents = businessEvents,
                ConceptValueTypes = conceptValueTypes,
                Formats = formats,
                Keywords = keywords,
                Structure = structure,
                Licenses = licenses,
                LifeEvents = lifeEvents,
                PublicationLevelProposals = levelProposals,
                PublicationLevels = levels,
                PublisherIdentifiers = publishers,
                RegistrationStatuses = statuses,
                RegistrationStatusProposals = statusProposals,
                Themes = themes,
                Types = types,
            },
            sort ?? CatalogSearchSort.Relevance,
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
    /// <param name="attributedAgents">Only results with a qualified attribution to one of the specified agents (by identifier) are counted</param>
    /// <param name="businessEvents">Only results corresponding to one of the specified business events are returned</param>
    /// <param name="conceptValueTypes">Only results corresponding to one of the specified IOP concept value types (CodeList, Date, Numeric, String) are counted.</param>
    /// <param name="formats">Only results with at least one distribution providing one of the specified formats are counted</param>
    /// <param name="keywords">Only results matching one of the specified keywords are counted</param>
    /// <param name="levels">Only results with one of the specified publication levels  are counted</param>
    /// <param name="levelProposals">Only results with the proposal publication is returned </param>
    /// <param name="licenses">Only results with at least one distribution providing one of the specified licenses are counted</param>
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
    public async Task<SearchCountResultModel> SearchCount(
        [FromQuery] string? language, 
        [FromQuery] string? query,
        [FromQuery] string[] accessRights, 
        [FromQuery] string[] attributedAgents,
        [FromQuery] string[] businessEvents, 
        [FromQuery] ConceptType[] conceptValueTypes, 
        [FromQuery] string[] formats,
        [FromQuery] string[] keywords,
        [FromQuery] PublicationLevel[] levels, 
        [FromQuery] PublicationLevel[] levelProposals,
        [FromQuery] string[] licenses,
        [FromQuery] string[] lifeEvents,
        [FromQuery] string[] publishers, 
        [FromQuery] RegistrationStatus[] statuses,
        [FromQuery] RegistrationStatus[] statusProposals,
        [FromQuery] SearchStructureOption? structure, 
        [FromQuery] string[] themes,
        [FromQuery] SearchResourceType[] types,
        CancellationToken cancellationToken)
    {
        var command = new GetCatalogSearchCountCommand(query, language, new()
        {
            AccessRights = accessRights,
            AttributedAgentIdentifiers = attributedAgents,
            BusinessEvents = businessEvents,
            ConceptValueTypes = conceptValueTypes,
            Formats = formats,
            Keywords = keywords,
            Licenses = licenses,
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

    /// <summary>
    /// Starts a full reindex in the IndexSearch service and returns at once; follow it with the status route.
    /// </summary>
    /// <exception cref="ForbiddenException"></exception>
    /// <exception cref="ConflictException"></exception>
    [HttpPost]
    [Route("reindex")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [Conflict]
    [InternalServerError]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Reindex(CancellationToken cancellationToken = default)
    {
        EnsureUserIsInteroperabilityService();

        try
        {
            var response = await _indexSearchApiClient.PostIndexReindexAsync(cancellationToken);

            return StatusCode(StatusCodes.Status202Accepted, response.Result);
        }
        catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status409Conflict)
        {
            throw new ConflictException("A reindex or another index operation is already running.");
        }
    }

    /// <summary>
    /// Says whether a reindex is running and how the last one ended.
    /// </summary>
    /// <exception cref="ForbiddenException"></exception>
    [HttpGet]
    [Route("reindex/status")]
    [Authorize]
    [Unauthorized]
    [Forbidden]
    [InternalServerError]
    [Ok(typeof(IndexStatusResponse))]
    public async Task<IndexStatusResponse> GetReindexStatus(CancellationToken cancellationToken = default)
    {
        EnsureUserIsInteroperabilityService();

        return (await _indexSearchApiClient.GetIndexStatusAsync(cancellationToken)).Result;
    }

    // The IndexSearch service receives the caller's token and checks the same role.
    private void EnsureUserIsInteroperabilityService()
    {
        if (_userContextService.GetUserBusinessRole() is not BusinessRole.InteroperabilityService)
        {
            throw new ForbiddenException("The user doesn't have the necessary rights.");
        }
    }
}
