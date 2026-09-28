using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ImportDatasetModelCommandHandler : IRequestHandler<ImportDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public ImportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(ImportDatasetModelCommand request, CancellationToken cancellationToken)
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
        var dataset = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        _catalogIndexService.UpdateIndex(dataset, hasStructure: true);

        return;
    }
}