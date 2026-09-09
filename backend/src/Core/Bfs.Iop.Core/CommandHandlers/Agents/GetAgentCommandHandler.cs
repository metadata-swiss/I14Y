using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class GetAgentCommandHandler : IRequestHandler<GetAgentCommand, AgentModel>
{
    private readonly IAgentsService _agentsService;

    public GetAgentCommandHandler(IAgentsService agentsService) => 
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task<AgentModel> Handle(GetAgentCommand request, CancellationToken cancellationToken) =>
        _agentsService.GetAgent(request.Id, cancellationToken);
}
