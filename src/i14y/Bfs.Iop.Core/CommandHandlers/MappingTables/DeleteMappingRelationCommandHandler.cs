using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class DeleteMappingRelationCommandHandler : IRequestHandler<DeleteMappingRelationCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public DeleteMappingRelationCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task Handle(DeleteMappingRelationCommand request, CancellationToken cancellationToken)
    {
        return _mappingTablesService.DeleteMappingRelation(request.MappingTableId, request.MappingRelationId, cancellationToken);
    }
}
