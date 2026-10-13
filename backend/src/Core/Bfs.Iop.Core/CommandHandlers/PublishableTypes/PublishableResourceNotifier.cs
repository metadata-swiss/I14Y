using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal sealed class PublishableResourceNotifier : IPublishableResourceNotifier
{
    private readonly IDatasetsService _datasetsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public PublishableResourceNotifier(
        IDatasetsService datasetsService,
        IPublicServicesService publicServicesService,
        IDataServicesService dataServicesService,
        IIopConceptsService iopConceptsService,
        IMappingTablesService mappingTablesService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _datasetsService = datasetsService ??
            throw new ArgumentNullException(nameof(datasetsService));

        _publicServicesService = publicServicesService ??
            throw new ArgumentNullException(nameof(publicServicesService));

        _dataServicesService = dataServicesService ??
            throw new ArgumentNullException(nameof(dataServicesService));

        _iopConceptsService = iopConceptsService ??
            throw new ArgumentNullException(nameof(iopConceptsService));

        _mappingTablesService = mappingTablesService ??
            throw new ArgumentNullException(nameof(mappingTablesService));

        _auditTrailNotifierService = auditTrailNotifierService ??
            throw new ArgumentNullException(nameof(auditTrailNotifierService));

        _searchIndexNotifier = searchIndexNotifier ??
            throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task NotifyUpdatedAsync(
        PublishableResourceType type,
        Guid id,
        CancellationToken cancellationToken)
    {
        switch (type)
        {
            case PublishableResourceType.Dataset:
                await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Dataset, id, cancellationToken);
                await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Dataset, id, cancellationToken);
                break;
            case PublishableResourceType.PublicService:
                await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.PublicService, id, cancellationToken);
                await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.PublicService, id, cancellationToken);
                break;
            case PublishableResourceType.DataService:
                await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DataService, id, cancellationToken);
                await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.DataService, id, cancellationToken);
                break;
            case PublishableResourceType.IopConcept:
                await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.Concept, id, cancellationToken);
                await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Concept, id, cancellationToken);
                break;
            case PublishableResourceType.MappingTable:
                await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTable, id, cancellationToken);
                await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.MappingTable, id, cancellationToken);
                break;
            default:
                throw new NotSupportedException($"The type '{type}' is not supported.");
        }
    }
}
