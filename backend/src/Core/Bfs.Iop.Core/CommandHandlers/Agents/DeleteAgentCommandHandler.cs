using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand>
{
    private readonly IAgentsService _agentsService;

    public DeleteAgentCommandHandler(IAgentsService agentsService) => 
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        return _agentsService.DeleteAgent(request.Id, cancellationToken);
    }
}
