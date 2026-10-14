using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateIopConceptVersionCommandHandler 
    : IRequestHandler<CreateIopConceptVersionCommand, Guid>
{
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public CreateIopConceptVersionCommandHandler(
        IIopConceptsService iopConceptsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task<Guid> Handle(CreateIopConceptVersionCommand request, CancellationToken cancellationToken)
    {
        var id = await _iopConceptsService.AddIopConceptVersion(request.PreviousId, request.Input, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.Concept, id, cancellationToken);

        var resource = await _iopConceptsService.GetIopConcept(id, false, cancellationToken);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Concept, id, cancellationToken);

        if (resource.ConceptType == ConceptType.CodeList)
        {
            await _searchIndexNotifier.NotifyCodeListChangedAsync(id, cancellationToken);
        }

        return id;
    }
}
