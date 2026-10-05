using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class UpdateIopConceptIsLockedCommandHandler : IRequestHandler<UpdateIopConceptIsLockedCommand>
{
    private readonly IIopConceptsService _conceptsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdateIopConceptIsLockedCommandHandler(
        IIopConceptsService iopConceptsService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _conceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _catalogIndexService = catalogIndexService 
            ?? throw new ArgumentNullException(nameof(catalogIndexService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));

        _searchIndexNotifier = searchIndexNotifier
            ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task Handle(UpdateIopConceptIsLockedCommand request, CancellationToken cancellationToken)
    {
        await _conceptsService.UpdateIsLocked(request.Id, request.Value, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, request.Id, cancellationToken);

        var resource = await _conceptsService.GetIopConcept(request.Id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Concept, request.Id, cancellationToken);
    }
}
