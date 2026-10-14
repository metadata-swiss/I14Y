using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteIopConceptCommandHandler : IRequestHandler<DeleteIopConceptCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeleteIopConceptCommandHandler(
        IIopConceptsService conceptsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));

        _searchIndexNotifier = searchIndexNotifier
            ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task Handle(DeleteIopConceptCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.Concept, request.Id, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.Id, cancellationToken);

        await _conceptsService.DeleteIopConcept(request.Id, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.Concept, request.Id, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.Id, cancellationToken);

        await _searchIndexNotifier.NotifyResourceDeletedAsync(request.Id, cancellationToken);

        await _searchIndexNotifier.NotifyCodeListDeletedAsync(request.Id, cancellationToken);
    }
}
