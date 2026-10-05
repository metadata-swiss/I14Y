using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class DeleteCodeListEntryCommandHandler : IRequestHandler<DeleteCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeleteCodeListEntryCommandHandler(
        IIopConceptsService conceptsService, 
        ICodeListEntryIndexService indexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task Handle(DeleteCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        await _conceptsService.DeleteCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        _indexService.DeIndex([request.CodeListEntryId]);

        await _searchIndexNotifier.NotifyCodeListChangedAsync(request.ConceptId, cancellationToken);
    }
}
