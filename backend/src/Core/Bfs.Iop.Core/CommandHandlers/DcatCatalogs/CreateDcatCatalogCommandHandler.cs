using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogCommandHandler : IRequestHandler<CreateDcatCatalogCommand, Guid>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateDcatCatalogCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<Guid> Handle(CreateDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        var id = await _dcatCatalogsService.AddDcatCatalog(request.InputModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.DcatCatalog, id, cancellationToken);

        return id;
    }
}
