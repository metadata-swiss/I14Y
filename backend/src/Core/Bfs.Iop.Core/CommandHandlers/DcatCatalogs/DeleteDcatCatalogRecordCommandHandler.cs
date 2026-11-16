using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class DeleteDcatCatalogRecordCommandHandler : AsyncRequestHandler<DeleteDcatCatalogRecordCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeleteDcatCatalogRecordCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    protected override async Task Handle(DeleteDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DcatCatalog, request.DcatCatalogId, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DcatCatalogRecords, request.DcatCatalogId, cancellationToken);

        var deleted = await _dcatCatalogsService.GetDcatCatalogRecord(request.DcatCatalogId, request.DcatCatalogRecordId, cancellationToken);

        await _dcatCatalogsService.DeleteDcatCatalogRecord(request.DcatCatalogId, request.DcatCatalogRecordId, cancellationToken);

        await NotifyDatasetsChangedAsync([deleted.PrimaryTopic], cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DcatCatalogRecords, request.DcatCatalogId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DcatCatalog, request.DcatCatalogId, cancellationToken);
    }

    private async Task NotifyDatasetsChangedAsync(IEnumerable<DcatCatalogResourceModel> resources, CancellationToken cancellationToken)
    {
        foreach (var datasetId in resources.Where(x => x.ResourceType == DcatCatalogType.Dataset).Select(x => x.ResourceId).Distinct())
        {
            await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Dataset, datasetId, cancellationToken);
        }
    }
}
