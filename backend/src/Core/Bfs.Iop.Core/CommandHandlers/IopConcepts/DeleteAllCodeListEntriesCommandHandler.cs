using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteAllCodeListEntriesCommandHandler : IRequestHandler<DeleteAllCodeListEntriesCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeleteAllCodeListEntriesCommandHandler(
        IIopConceptsService conceptsService,
        ICodeListEntryIndexService indexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));

        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
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

        // The concept survives; only its entries are gone. Replacing the concept with what it now
        // holds - nothing - is the same operation as any other code list change.
        await _searchIndexNotifier.NotifyCodeListChangedAsync(request.ConceptId, cancellationToken);
    }
}
