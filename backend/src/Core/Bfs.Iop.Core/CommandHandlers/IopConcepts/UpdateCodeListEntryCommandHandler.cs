using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateCodeListEntryCommandHandler : AsyncRequestHandler<UpdateCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdateCodeListEntryCommandHandler(
        IIopConceptsService conceptsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    protected override async Task Handle(UpdateCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateCodeListEntry(
            request.ConceptId, 
            request.CodeListEntryId,
            request.UpdateModel,
            cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        await _searchIndexNotifier.NotifyCodeListChangedAsync(request.ConceptId, cancellationToken);
    }
}
