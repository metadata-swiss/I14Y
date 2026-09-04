using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingTableVersionCommandHandler : IRequestHandler<CreateMappingTableVersionCommand, Guid>
{
    private readonly IMappingTablesService _mappingTablesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public CreateMappingTableVersionCommandHandler(
        IMappingTablesService mappingTablesService,
        ICatalogIndexService catalogIndexService)
    {
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task<Guid> Handle(CreateMappingTableVersionCommand request, CancellationToken cancellationToken)
    {
        var id = await _mappingTablesService.AddMappingTableVersion(request.Id, request.InputModel, cancellationToken);

        var resource = await _mappingTablesService.GetMappingTable(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        return id;
    }
}
