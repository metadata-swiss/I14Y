using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class GetAgentParentAgentsCommandHandler : IRequestHandler<GetAgentParentAgentsCommand, IEnumerable<AgentModel>>
{
    private readonly IAgentsService _agentsService;

    public GetAgentParentAgentsCommandHandler(IAgentsService agentsService) => 
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task<IEnumerable<AgentModel>> Handle(GetAgentParentAgentsCommand request, CancellationToken cancellationToken) => 
        _agentsService.GetAgentParentAgents(request.Id, cancellationToken);
}
