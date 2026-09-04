using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand>
{
    private readonly IAgentsService _agentsService;

    public UpdateAgentCommandHandler(IAgentsService agentsService) =>
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        return _agentsService.UpdateAgent(request.Id, request.UpdateModel, cancellationToken);
    }
}
