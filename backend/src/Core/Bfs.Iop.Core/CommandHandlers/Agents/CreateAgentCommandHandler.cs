using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IAgentsService _agentsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateAgentCommandHandler(
        IAgentsService agentsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var id = await _agentsService.AddAgent(request.InputModel, cancellationToken);

        return id;
    }
}
