using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand>
{
    private readonly IAgentsService _agentsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateAgentCommandHandler(
        IAgentsService agentsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        // Resolved before the update: a technical client's author is read from its own agent, which may be the one being updated.
        var author = await _auditTrailNotifierService.GetAuthorAsync(cancellationToken);

        await _agentsService.UpdateAgent(request.Id, request.UpdateModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Agent, request.Id, author, cancellationToken);
    }
}
