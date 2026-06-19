using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class GetAgentsCommandHandler : IRequestHandler<GetAgentsCommand, PagedResult<AgentModel>>
{
    private readonly IAgentsService _agentsService;

    public GetAgentsCommandHandler(IAgentsService agentsService) =>
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task<PagedResult<AgentModel>> Handle(GetAgentsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _agentsService.GetAgents(request.Identifier, request.Uid, page, pageSize, cancellationToken);
    }
}
