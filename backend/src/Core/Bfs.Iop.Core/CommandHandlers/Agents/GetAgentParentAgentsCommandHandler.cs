using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class GetAgentParentAgentsCommandHandler : IRequestHandler<GetAgentParentAgentsCommand, IEnumerable<AgentModel>>
{
    private readonly IAgentsService _agentsService;

    public GetAgentParentAgentsCommandHandler(IAgentsService agentsService) => 
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public async Task<IEnumerable<AgentModel>> Handle(GetAgentParentAgentsCommand request, CancellationToken cancellationToken)
    {
        var agent = await _agentsService.GetAgent(request.Id, cancellationToken);

        return agent.SubOrganizationOf is null
            ? []
            : new[] { await _agentsService.GetAgent(agent.SubOrganizationOf.Id, cancellationToken) };
    }
}
