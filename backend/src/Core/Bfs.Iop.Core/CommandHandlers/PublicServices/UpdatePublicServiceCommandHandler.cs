using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class UpdatePublicServiceCommandHandler : IRequestHandler<UpdatePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdatePublicServiceCommandHandler(
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

    public async Task Handle(UpdatePublicServiceCommand request, CancellationToken cancellationToken)
    {
        await _publicServicesService.UpdatePublicService(request.Id, request.Model, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.PublicService, request.Id, cancellationToken);

        var resource = await _publicServicesService.GetPublicService(request.Id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.PublicService, request.Id, cancellationToken);
    }
}
