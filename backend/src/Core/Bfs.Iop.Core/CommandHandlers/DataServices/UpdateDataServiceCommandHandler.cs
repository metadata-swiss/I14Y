using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class UpdateDataServiceCommandHandler : IRequestHandler<UpdateDataServiceCommand>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDataServiceCommandHandler(
        IDataServicesService dataServicesService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(UpdateDataServiceCommand request, CancellationToken cancellationToken)
    {
        await _dataServicesService.UpdateDataService(request.Id, request.InputModel, cancellationToken);

        var resource = await _dataServicesService.GetDataService(request.Id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
