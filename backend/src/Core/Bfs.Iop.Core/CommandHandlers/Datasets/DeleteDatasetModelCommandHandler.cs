using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetModelCommandHandler : AsyncRequestHandler<DeleteDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public DeleteDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService ?? 
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));

        _searchIndexNotifier = searchIndexNotifier
            ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    protected override async Task Handle(DeleteDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
        

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Dataset, request.DatasetId, cancellationToken);

        return;
    }
}
