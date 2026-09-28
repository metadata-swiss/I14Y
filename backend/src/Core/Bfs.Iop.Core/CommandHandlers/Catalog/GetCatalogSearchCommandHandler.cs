using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

using CatalogSearchRequest = Bfs.Iop.IndexSearch.ApiClient.CatalogSearchRequest;
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

        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, DefaultPageSize);

        var response = await _search.PostSearchCatalogByBodyAsync(
            new CatalogSearchRequest
            {
                Query = request.Query,
                Language = request.Language,
                Filter = request.Filter.MapToIndexSearchFilter(),
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken);

        var hits = response.Result.Results ?? [];

        var agents = await Agents(hits, cancellationToken);

        return new PagedResult<SearchResultModel>
        {
            Page = response.Result.Page ?? page,
            PageSize = response.Result.PageSize ?? pageSize,
            TotalCount = response.Result.TotalCount ?? 0,

            Results = hits
                .Where(x => x.PublisherId.HasValue && agents.ContainsKey(x.PublisherId.Value))
                .Select(x => x.MapToSearchResultModel(agents[x.PublisherId!.Value], _vocabulariesService))
                .ToList()
                .AsReadOnly(),
        };
    }

    /// <summary>
    ///     Every publisher on the page, in one call. IAgentsService resolves a whole set, so asking
    ///     for them one at a time would be a round trip per distinct publisher.
    /// </summary>
    private async Task<Dictionary<Guid, AgentModel>> Agents(
        IEnumerable<IndexSearch.ApiClient.CatalogSearchHit> hits,
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
