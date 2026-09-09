using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class UpdateMappingTableCommandHandler : IRequestHandler<UpdateMappingTableCommand>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdateMappingTableCommandHandler(
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdateMappingTableCommand request, CancellationToken cancellationToken)
    {
        await _mappingTablesService.UpdateMappingTable(request.Id, request.InputModel, cancellationToken);

        var resource = await _mappingTablesService.GetMappingTable(request.Id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
