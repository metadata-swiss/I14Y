using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class CreateDataServiceCommandHandler : IRequestHandler<CreateDataServiceCommand, Guid>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public CreateDataServiceCommandHandler(
        IDataServicesService dataServicesService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task<Guid> Handle(CreateDataServiceCommand request, CancellationToken cancellationToken)
    {
        var id = await _dataServicesService.AddDataService(request.InputModel, cancellationToken);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.DataService, id, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.DataService, id, cancellationToken);

        return id;
    }
}
