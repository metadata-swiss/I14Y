using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteAllMappingRelationsCommandHandler : IRequestHandler<DeleteAllMappingRelationsCommand>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public DeleteAllMappingRelationsCommandHandler(
        IMappingTablesService mappingTablesService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(DeleteAllMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.MappingTableRelations, request.MappingTableId, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.MappingTable, request.MappingTableId, cancellationToken);

        await _mappingTablesService.DeleteAllMappingRelations(request.MappingTableId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceDeletedAsync(AuditTrailResourceType.MappingTableRelations, request.MappingTableId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTable, request.MappingTableId, cancellationToken);
    }
}
