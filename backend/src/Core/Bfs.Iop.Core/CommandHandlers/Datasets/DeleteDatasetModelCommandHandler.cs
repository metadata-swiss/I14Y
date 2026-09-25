using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetModelCommandHandler : IRequestHandler<DeleteDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public DeleteDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService ?? 
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(DeleteDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
        
        _catalogIndexService.DeIndex(request.DatasetId);

        return;
    }
}
