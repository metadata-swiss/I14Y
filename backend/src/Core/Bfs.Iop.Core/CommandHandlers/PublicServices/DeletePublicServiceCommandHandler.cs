using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class DeletePublicServiceCommandHandler : IRequestHandler<DeletePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeletePublicServiceCommandHandler(
        IPublicServicesService publicServicesService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task Handle(DeletePublicServiceCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.PublicService, request.Id, cancellationToken);

        await _publicServicesService.DeletePublicService(request.Id, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.PublicService, request.Id, cancellationToken);

        _catalogIndexService.DeIndex(request.Id);

        await _searchIndexNotifier.NotifyResourceDeletedAsync(request.Id, cancellationToken);
    }
}
