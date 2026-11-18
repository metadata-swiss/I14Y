using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class DeleteDcatCatalogCommandHandler : AsyncRequestHandler<DeleteDcatCatalogCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;
    private readonly ISearchIndexProviderService _searchIndexProvider;

    public DeleteDcatCatalogCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier,
        ISearchIndexProviderService searchIndexProvider)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
        _searchIndexProvider = searchIndexProvider ?? throw new ArgumentNullException(nameof(searchIndexProvider));
    }

    protected override async Task Handle(DeleteDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DcatCatalog, request.Id, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DcatCatalogRecords, request.Id, cancellationToken);

        var resources = await _searchIndexProvider.GetDcatCatalogResources(request.Id, cancellationToken);

        await _dcatCatalogsService.DeleteDcatCatalog(request.Id, cancellationToken);

        await _searchIndexNotifier.NotifyDcatCatalogResourcesChangedAsync(resources, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.DcatCatalogRecords, request.Id, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.DcatCatalog, request.Id, cancellationToken);
    }
}
