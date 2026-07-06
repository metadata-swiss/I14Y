using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class GetMappingTableCommandHandler : IRequestHandler<GetMappingTableCommand, MappingTableModel>
{
    private readonly IMappingTablesService _mappingTablesService;

    public GetMappingTableCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<MappingTableModel> Handle(GetMappingTableCommand request, CancellationToken cancellationToken) =>
        _mappingTablesService.GetMappingTable(request.Id, cancellationToken);
}
