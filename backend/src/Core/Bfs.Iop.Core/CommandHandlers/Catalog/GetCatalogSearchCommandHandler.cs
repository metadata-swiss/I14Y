using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

using Client = Bfs.Iop.IndexSearch.ApiClient;
using IIndexSearchApiClient = Bfs.Iop.IndexSearch.ApiClient.IIndexSearchApiClient;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetCatalogSearchCommandHandler
    : IRequestHandler<GetCatalogSearchCommand, PagedResult<SearchResultModel>>
{
 
    internal const int DefaultPageSize = 200;

    private readonly IIndexSearchApiClient _search;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IAgentsService _agentsService;

    public GetCatalogSearchCommandHandler(
        IIndexSearchApiClient search,
        IVocabulariesService vocabulariesService,
        IAgentsService agentsService)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));

        _vocabulariesService = vocabulariesService ??
            throw new ArgumentNullException(nameof(vocabulariesService));

        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
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
            Results = found.Hits
                .Where(x => x.PublisherId.HasValue && agents.ContainsKey(x.PublisherId.Value))
                .Select(x => x.MapToSearchResultModel(agents[x.PublisherId!.Value], _vocabulariesService))
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
        var collected = new List<Client.CatalogSearchHit>();
        var page = 1;
        var total = 0;

        while (true)
        {
            var result = await SearchAsync(request, filter, page, DefaultPageSize, cancellationToken);

            var hits = result.Results ?? [];

            total = result.TotalCount ?? collected.Count;
            collected.AddRange(hits);

            if (collected.Count >= total)
            {
                break;
            }

            if (hits.Count == 0)
            {
                throw new InvalidOperationException(
                    $"The search matched {total} resources but the index served only {collected.Count} "
                    + "of them. Ask for a page, or narrow the search.");
            }

            page++;
        }

        return new Found(collected, 1, total, total);
    }

    private async Task<Client.CatalogSearchHitPagedResult> SearchAsync(
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
    private async Task<Dictionary<Guid, AgentModel>> Agents(
        IEnumerable<Client.CatalogSearchHit> hits,
        CancellationToken cancellationToken)
    {
        var ids = hits.Select(x => x.PublisherId).OfType<Guid>().Distinct().ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        var agents = await _agentsService.GetAgents(ids, cancellationToken);

        return agents.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
    }
}
