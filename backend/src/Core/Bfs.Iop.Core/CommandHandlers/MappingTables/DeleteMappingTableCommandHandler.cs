using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteMappingTableCommandHandler : IRequestHandler<DeleteMappingTableCommand>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public DeleteMappingTableCommandHandler(
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(DeleteMappingTableCommand request, CancellationToken cancellationToken)
    {
        await _mappingTablesService.DeleteMappingTable(request.Id, cancellationToken);

        _catalogIndexService.DeIndex(request.Id);
    }
}
