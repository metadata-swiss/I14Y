using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class UpdateDatasetModelClassCommandHandler : IRequestHandler<UpdateDatasetModelClassCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDatasetModelClassCommandHandler(
        IDatasetModelProcessService datasetModelProcessService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateDatasetModelClassCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        await _datasetModelProcessService.UpdateSchemaClass(request.DatasetId, request.SchemaClassInput, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
    }
}
