using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateCodeListEntriesCommandHandler : IRequestHandler<CreateCodeListEntriesCommand, IEnumerable<Guid>>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateCodeListEntriesCommandHandler(
        IIopConceptsService iopConceptsService,
        ICodeListEntryIndexService indexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _indexService = indexService ?? throw new ArgumentNullException(nameof(indexService));

        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<IEnumerable<Guid>> Handle(CreateCodeListEntriesCommand request, CancellationToken cancellationToken)
    {
        var ids = await _conceptsService.AddCodeListEntries(request.ConceptId, request.InputModels, cancellationToken);

        var concept = await _conceptsService.GetIopConcept(request.ConceptId, includeCodeListEntries: true, cancellationToken);

        _indexService.UpdateIndex(concept.CodeListEntries!);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.ConceptId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.ConceptCodeListEntries, request.ConceptId, cancellationToken);

        return ids;
    }
}


