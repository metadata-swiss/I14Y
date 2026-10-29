using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ImportDatasetModelCommandHandler : AsyncRequestHandler<ImportDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public ImportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));

        _searchIndexNotifier = searchIndexNotifier
            ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    protected override async Task Handle(ImportDatasetModelCommand request, CancellationToken cancellationToken)
    {
        var structureExists = await _datasetModelFileProcessService.GraphExists(request.DatasetId, cancellationToken);

        if (structureExists)
        {
            await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
        }

        await _datasetModelFileProcessService.UploadGraph(request.ImportFile, request.DatasetId, cancellationToken);

        if (structureExists)
        {
            await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
        }
        else
        {
            await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
        }

        // Update index
        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.Dataset, request.DatasetId, cancellationToken);

        return;
    }
}