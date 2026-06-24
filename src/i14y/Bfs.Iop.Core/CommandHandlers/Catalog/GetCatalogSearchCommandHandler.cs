using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

internal sealed class GetCatalogSearchCommandHandler : IRequestHandler<GetCatalogSearchCommand, PagedResult<SearchResultModel>>
{

    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IAgentsService _agentsService;

    public GetCatalogSearchCommandHandler(
        ICatalogIndexService catalogIndexService,
        IVocabulariesService vocabulariesService,
        IAgentsService agentsService)
    {
        _catalogIndexService = catalogIndexService ??
            throw new ArgumentNullException(nameof(catalogIndexService));

        _vocabulariesService = vocabulariesService ??
            throw new ArgumentNullException(nameof(vocabulariesService));

        _agentsService = agentsService ??
            throw new ArgumentNullException(nameof(agentsService));
    }

    public async Task<PagedResult<SearchResultModel>> Handle(GetCatalogSearchCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        var pagedItems = _catalogIndexService.Search(request.Query, request.Language, request.Filter, page, pageSize);

        var agents = await GetAgents(pagedItems.Results.Select(x => x.Publisher).Distinct(), cancellationToken);

        var pagedResults = new PagedResult<SearchResultModel>()
        {
            Page = pagedItems.Page,
            PageSize = pagedItems.PageSize,
            Results = pagedItems.Results.Select(x => x.MapToSearchResultModel(
                agents.Single(y => y.Id == x.Publisher),
                _vocabulariesService)),
            TotalCount = pagedItems.TotalCount,
        };

        return pagedResults;
    }

    private async Task<IEnumerable<AgentModel>> GetAgents(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var agents = new List<AgentModel>();

        foreach (var id in ids)
        {
            agents.Add(await _agentsService.GetAgent(id, cancellationToken));
        }

        return agents;
    }
}
