using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogRecordsCommandHandler : IRequestHandler<CreateDcatCatalogRecordsCommand, IEnumerable<Guid>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateDcatCatalogRecordsCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<IEnumerable<Guid>> Handle(CreateDcatCatalogRecordsCommand request, CancellationToken cancellationToken)
    {
        var ids = await _dcatCatalogsService.AddDcatCatalogRecords(request.DcatCatalogId, request.InputModels, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.DcatCatalogRecords, request.DcatCatalogId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.DcatCatalog, request.DcatCatalogId, cancellationToken);

        return ids;
    }
}