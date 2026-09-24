using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingTableCommandHandler : IRequestHandler<CreateMappingTableCommand, Guid>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;

    public CreateMappingTableCommandHandler(
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
    }

    public async Task<Guid> Handle(CreateMappingTableCommand request, CancellationToken cancellationToken)
    {
        var id = await _mappingTablesService.AddMappingTable(request.InputModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.MappingTable, id, cancellationToken);

        var resource = await _mappingTablesService.GetMappingTable(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        return id;
    }
}
