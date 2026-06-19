using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class UpdateMappingRelationCommandHandler : IRequestHandler<UpdateMappingRelationCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public UpdateMappingRelationCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task Handle(UpdateMappingRelationCommand request, CancellationToken cancellationToken)
    {
        return _mappingTablesService.UpdateMappingRelation(
            request.MappingTableId, 
            request.MappingRelationId, 
            request.InputModel,
            cancellationToken);
    }
}
