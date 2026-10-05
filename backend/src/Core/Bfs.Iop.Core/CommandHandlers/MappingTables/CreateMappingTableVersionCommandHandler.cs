using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingTableVersionCommandHandler : IRequestHandler<CreateMappingTableVersionCommand, Guid>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IAuditTrailNotifierService _auditTrailNotifierService;
    private readonly ISearchIndexNotifierService _searchIndexNotifier;

    public CreateMappingTableVersionCommandHandler(
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService,
        IAuditTrailNotifierService auditTrailNotifierService,
        ISearchIndexNotifierService searchIndexNotifier)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
        _auditTrailNotifierService = auditTrailNotifierService ?? throw new ArgumentNullException(nameof(auditTrailNotifierService));
        _searchIndexNotifier = searchIndexNotifier ?? throw new ArgumentNullException(nameof(searchIndexNotifier));
    }

    public async Task<Guid> Handle(CreateMappingTableVersionCommand request, CancellationToken cancellationToken)
    {
        var id = await _mappingTablesService.AddMappingTableVersion(request.Id, request.InputModel, cancellationToken);

        await _auditTrailNotifierService.NotifyResourceCreatedAsync(AuditTrailResourceType.MappingTable, id, cancellationToken);

        var resource = await _mappingTablesService.GetMappingTable(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        await _searchIndexNotifier.NotifyResourceChangedAsync(SearchResourceType.MappingTable, id, cancellationToken);

        return id;
    }
}
