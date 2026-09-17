using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand>
{
    private readonly IAgentsService _agentsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public DeleteAgentCommandHandler(
        IAgentsService agentsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.Agent, request.Id, cancellationToken);

        await _agentsService.DeleteAgent(request.Id, cancellationToken);

        _ = _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.Agent, request.Id, cancellationToken);
    }
}
