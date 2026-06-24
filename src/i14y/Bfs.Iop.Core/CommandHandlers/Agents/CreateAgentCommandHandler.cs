using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IAgentsService _agentsService;

    public CreateAgentCommandHandler(IAgentsService agentsService) =>
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

    public Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken) =>
        _agentsService.AddAgent(request.InputModel, cancellationToken);
}
