using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteMappingRelationCommandHandler : IRequestHandler<DeleteMappingRelationCommand>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public DeleteMappingRelationCommandHandler(
        IMappingTablesService mappingTablesService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task Handle(DeleteMappingRelationCommand request, CancellationToken cancellationToken)
    {
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.MappingTableRelations, request.MappingTableId, cancellationToken);
        await _auditTrailNotifierService.EnsureResourceIsTrackedAsync(AuditTrailResourceType.MappingTable, request.MappingTableId, cancellationToken);

        await _mappingTablesService.DeleteMappingRelation(request.MappingTableId, request.MappingRelationId, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTableRelations, request.MappingTableId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTable, request.MappingTableId, cancellationToken);
    }
}
