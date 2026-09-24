using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Messaging.AuditTrail;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class CreateDatasetModelPropertyCommandHandler : IRequestHandler<CreateDatasetModelPropertyCommand, Uri>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateDatasetModelPropertyCommandHandler(
        IDatasetModelProcessService datasetModelProcessService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));

        _auditTrailNotifierService = auditTrailNotifierService
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<Uri> Handle(CreateDatasetModelPropertyCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        var uri = await _datasetModelProcessService.CreateSchemaProperty(
            request.DatasetId,          
            request.PropertyInput,
            request.ClassUri,
            cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DatasetStructure, request.DatasetId, cancellationToken);

        return uri;
    }
}
