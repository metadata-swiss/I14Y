using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteAllCodeListEntriesCommandHandler : IRequestHandler<DeleteAllCodeListEntriesCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public DeleteAllCodeListEntriesCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexService indexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));

        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(DeleteAllCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        var ids = (await _conceptsService.GetCodeListEntries(
            request.ConceptId,
            sortProperty: null,
            page: 1,
            pageSize: int.MaxValue,
            sortOrder: DataAccess.Abstractions.SortOrder.Ascending, 
            cancellationToken: cancellationToken)).Results.Select(x => x.Id);

        await _conceptsService.DeleteAllCodeListEntriesFromIopConcept(request.ConceptId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        _indexService.DeIndex(ids);
    }
}
