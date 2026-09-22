using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingRelationsCommandHandler : IRequestHandler<CreateMappingRelationsCommand, IEnumerable<Guid>>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateMappingRelationsCommandHandler(
        IMappingTablesService mappingTablesService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _mappingTablesService = mappingTablesService
            ?? throw new ArgumentNullException(nameof(mappingTablesService));

        _auditTrailNotifierService = auditTrailNotifierService 
            ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<IEnumerable<Guid>> Handle(CreateMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        var ids = await _mappingTablesService.AddRelations(request.MappingTableId, request.InputModels, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.MappingTableRelations, request.MappingTableId, cancellationToken);
        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTable, request.MappingTableId, cancellationToken);

        return ids;
    }
}
