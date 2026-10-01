using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateCodeListEntryCommandHandler : IRequestHandler<UpdateCodeListEntryCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdateCodeListEntryCommandHandler(
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

    public async Task Handle(UpdateCodeListEntryCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateCodeListEntry(
            request.ConceptId, 
            request.CodeListEntryId,
            request.UpdateModel,
            cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);

        var entry = await _conceptsService.GetCodeListEntry(request.ConceptId, request.CodeListEntryId, cancellationToken);

        _indexService.UpdateIndex([entry]);

        await _searchIndexNotifier.NotifyCodeListChangedAsync(request.ConceptId, cancellationToken);
    }
}
