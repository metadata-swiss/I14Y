using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class UpdateDatasetModelPropertyCommandHandler : IRequestHandler<UpdateDatasetModelPropertyCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDatasetModelPropertyCommandHandler(
        IDatasetModelProcessService datasetModelProcessService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateDatasetModelPropertyCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        await _datasetModelProcessService.UpdateSchemaProperty(
            request.DatasetId,
            request.PropertyInput,
            request.ClassUri,
            cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
    }
}
