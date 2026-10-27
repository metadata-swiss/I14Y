using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class UpdateDatasetModelPositionCommandHandler : IRequestHandler<UpdateDatasetModelPositionCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDatasetModelPositionCommandHandler(
        IDatasetModelProcessService datasetModelProcessService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateDatasetModelPositionCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        await _datasetModelProcessService.UpdateClassesPosition(request.DatasetId, request.ClassesPositionInput, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);
    }
}
