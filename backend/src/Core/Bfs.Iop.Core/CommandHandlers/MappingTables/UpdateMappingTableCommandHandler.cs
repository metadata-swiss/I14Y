using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class UpdateMappingTableCommandHandler : AsyncRequestHandler<UpdateMappingTableCommand>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public UpdateMappingTableCommandHandler(
        IMappingTablesService mappingTablesService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    protected override async Task Handle(UpdateMappingTableCommand request, CancellationToken cancellationToken)
    {
        await _mappingTablesService.UpdateMappingTable(request.Id, request.InputModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceUpdatedAsync(AuditTrailResourceType.MappingTable, request.Id, cancellationToken);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.MappingTable, request.Id, cancellationToken);
    }
}
