using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class UpdateDatasetCommandHandler : IRequestHandler<UpdateDatasetCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDatasetCommandHandler(
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateDatasetCommand request, CancellationToken cancellationToken)
    {
        await _datasetsService.UpdateDataset(request.DatasetId, request.DatasetInput, cancellationToken);

        var resource = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
