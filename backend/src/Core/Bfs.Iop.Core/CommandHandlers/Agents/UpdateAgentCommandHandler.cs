using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class UpdateAgentCommandHandler : AsyncRequestHandler<UpdateAgentCommand>
{
    private readonly IAgentsService _agentsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;
    private readonly ISearchIndexProviderService _searchIndexProvider;

    public UpdateAgentCommandHandler(
        IAgentsService agentsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier,
        ISearchIndexProviderService searchIndexProvider)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
        _searchIndexProvider = searchIndexProvider ?? throw new ArgumentNullException(nameof(searchIndexProvider));
    }

    protected override async Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        await _agentsService.UpdateAgent(request.Id, request.UpdateModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Agent, request.Id, cancellationToken);

        var resources = await _searchIndexProvider.GetCatalogResourcesReferencingAgent(request.Id, cancellationToken);

        foreach (var resource in resources)
        {
            await _searchIndexNotifier.NotifyResourceChangedAsync(resource.Type, resource.Id, cancellationToken);
        }
    }
}
