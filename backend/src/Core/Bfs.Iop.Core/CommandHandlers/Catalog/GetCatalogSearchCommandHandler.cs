using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

using Client = Bfs.Iop.IndexSearch.Contracts.Search;
using ApiClient = Bfs.Iop.IndexSearch.ApiClient;
using IIndexSearchApiClient = Bfs.Iop.IndexSearch.ApiClient.IIndexSearchApiClient;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetCatalogSearchCommandHandler
    : IRequestHandler<GetCatalogSearchCommand, PagedResult<SearchResultModel>>
{
 
    private readonly IIndexSearchApiClient _search;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IAgentsService _agentsService;
    private readonly ILogger<GetCatalogSearchCommandHandler> _logger;

    public GetCatalogSearchCommandHandler(
        IIndexSearchApiClient search,
        IVocabulariesService vocabulariesService,
        IAgentsService agentsService,
        ILogger<GetCatalogSearchCommandHandler> logger)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));

        _vocabulariesService = vocabulariesService ??
            throw new ArgumentNullException(nameof(vocabulariesService));

        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<SearchResultModel>> Handle(
        GetCatalogSearchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var filter = request.Filter.MapToIndexSearchFilter();

        var found = request.Page.HasValue && request.PageSize.HasValue
            ? await OnePageAsync(request, filter, request.Page.Value, request.PageSize.Value, cancellationToken)
            : await EveryPageAsync(request, filter, cancellationToken);

        var agents = await Agents(found.Hits, cancellationToken);

        return new PagedResult<SearchResultModel>
        {
            Page = found.Page,
            PageSize = found.PageSize,
            TotalCount = found.TotalCount,

            // A hit whose publisher the agents service cannot resolve is dropped rather than failing
            // the page, matching GetCatalogSearchCountCommandHandler: the model requires a publisher,
            // and one unresolvable row is a data problem, not a reason to show the caller nothing.
            Results = Resolvable(found.Hits, agents)
                .Select(x => x.MapToSearchResultModel(agents[x.PublisherId], _vocabulariesService))
                .ToList()
                .AsReadOnly(),
        };
    }

    private async Task<Found> OnePageAsync(
        GetCatalogSearchCommand request,
        Client.CatalogSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await SearchAsync(request, filter, page, pageSize, cancellationToken);

        return new Found(
            [.. result.Results ?? []],
            result.Page ?? page,
            result.PageSize ?? pageSize,
            result.TotalCount ?? 0);
    }

    private async Task<Found> EveryPageAsync(
        GetCatalogSearchCommand request,
        Client.CatalogSearchFilter filter,
        CancellationToken cancellationToken)
    {
        var response = await _search.PostSearchCatalogAllByBodyAsync(
            new Client.CatalogSearchRequest
            {
                Query = request.Query,
                Language = request.Language,
                Filter = filter,
            },
            cancellationToken);

        var all = response.Result is null ? [] : response.Result.ToList().AsReadOnly();

        return new Found(all, 1, all.Count, all.Count);
    }

    private async Task<ApiClient.CatalogSearchHitPagedResult> SearchAsync(
        GetCatalogSearchCommand request,
        Client.CatalogSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _search.PostSearchCatalogByBodyAsync(
            new Client.CatalogSearchRequest
            {
                Query = request.Query,
                Language = request.Language,
                Filter = filter,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken);

        return response.Result;
    }

    private sealed record Found(
        IReadOnlyCollection<Client.CatalogSearchHit> Hits,
        int Page,
        int PageSize,
        int TotalCount);

    /// <summary>
    ///     Every publisher on the page, in one call. IAgentsService resolves a whole set, so asking
    ///     for them one at a time would be a round trip per distinct publisher.
    /// </summary>
    /// <summary>
    ///     The hits whose publisher the database still knows, reporting the ones it does not.
    /// </summary>
    /// <remarks>
    ///     A publisher cannot be deleted while any resource references it, so an unresolvable one
    ///     means the index holds a document the database no longer backs - a removal that never
    ///     arrived. Dropping it silently throws away the only signal of that; the next rebuild clears
    ///     the document itself.
    /// </remarks>
    private IEnumerable<Client.CatalogSearchHit> Resolvable(
        IEnumerable<Client.CatalogSearchHit> hits,
        Dictionary<Guid, AgentModel> agents)
    {
        foreach (var hit in hits)
        {
            if (agents.ContainsKey(hit.PublisherId))
            {
                yield return hit;

                continue;
            }

            _logger.LogWarning(
                "The catalogue index holds '{Id}' with publisher '{PublisherId}', which the database "
                + "does not know. The resource was left out of the results; the next rebuild removes "
                + "it from the index.",
                hit.Id,
                hit.PublisherId);
        }
    }

    private async Task<Dictionary<Guid, AgentModel>> Agents(
        IEnumerable<Client.CatalogSearchHit> hits,
        CancellationToken cancellationToken)
    {
        var ids = hits.Select(x => x.PublisherId).Distinct().ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        var agents = await _agentsService.GetAgents(ids, cancellationToken);

        return agents.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
    }
}
