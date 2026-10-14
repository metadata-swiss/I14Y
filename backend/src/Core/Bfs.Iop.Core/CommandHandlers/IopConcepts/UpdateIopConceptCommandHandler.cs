using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptCommandHandler : IRequestHandler<UpdateIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdateIopConceptCommandHandler(
        IIopConceptsService conceptsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task Handle(UpdateIopConceptCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateConcept(request.Id, request.UpdateModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.Id, cancellationToken);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Concept, request.Id, cancellationToken);
    }
}
