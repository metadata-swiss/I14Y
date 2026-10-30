using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class UpdateDcatCatalogCommandHandler : AsyncRequestHandler<UpdateDcatCatalogCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public UpdateDcatCatalogCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    protected override async Task Handle(UpdateDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        await _dcatCatalogsService.UpdateDcatCatalog(request.Id, request.UpdateModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DcatCatalog, request.Id, cancellationToken);
    }
}
