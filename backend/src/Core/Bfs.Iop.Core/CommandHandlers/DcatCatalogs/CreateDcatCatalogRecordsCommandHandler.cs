using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogRecordsCommandHandler : IRequestHandler<CreateDcatCatalogRecordsCommand, IEnumerable<Guid>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public CreateDcatCatalogRecordsCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task<IEnumerable<Guid>> Handle(CreateDcatCatalogRecordsCommand request, CancellationToken cancellationToken)
    {
        var ids = await _dcatCatalogsService.AddDcatCatalogRecords(request.DcatCatalogId, request.InputModels, cancellationToken);

        await NotifyDatasetsChangedAsync(request.InputModels.Select(x => x.PrimaryTopic), cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.DcatCatalogRecords, request.DcatCatalogId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DcatCatalog, request.DcatCatalogId, cancellationToken);

        return ids;
    }

    private async Task NotifyDatasetsChangedAsync(IEnumerable<DcatCatalogResourceModel> resources, CancellationToken cancellationToken)
    {
        foreach (var datasetId in resources.Where(x => x.ResourceType == DcatCatalogType.Dataset).Select(x => x.ResourceId).Distinct())
        {
            await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Dataset, datasetId, cancellationToken);
        }
    }
}