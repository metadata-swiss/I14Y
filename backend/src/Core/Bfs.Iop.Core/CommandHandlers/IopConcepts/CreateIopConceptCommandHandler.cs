using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class CreateIopConceptCommandHandler : IRequestHandler<CreateIopConceptCommand, Guid>
{
    private readonly IIopConceptsService _iopConceptsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public CreateIopConceptCommandHandler(
        IIopConceptsService iopConceptsService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task<Guid> Handle(CreateIopConceptCommand request, CancellationToken cancellationToken)
    {
        var id = await _iopConceptsService.AddIopConcept(request.Input, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.Concept, id, cancellationToken);

        var resource = await _iopConceptsService.GetIopConcept(id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Concept, id, cancellationToken);

        return id;
    }
}
