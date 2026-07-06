using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteMappingTableCommandHandler : IRequestHandler<DeleteMappingTableCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public DeleteMappingTableCommandHandler(IMappingTablesService mappingTablesService) => _mappingTablesService = mappingTablesService ?? 
        throw new ArgumentNullException(nameof(mappingTablesService));

    public Task Handle(DeleteMappingTableCommand request, CancellationToken cancellationToken)
    {
        return _mappingTablesService.DeleteMappingTable(request.Id, cancellationToken);
    }
}
