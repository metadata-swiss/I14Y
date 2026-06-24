using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteAllMappingRelationsCommandHandler : IRequestHandler<DeleteAllMappingRelationsCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public DeleteAllMappingRelationsCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task Handle(DeleteAllMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        return _mappingTablesService.DeleteAllMappingRelations(request.MappingTableId, cancellationToken);
    }
}
