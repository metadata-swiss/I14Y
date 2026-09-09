using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class GetMappingRelationCommandHandler : IRequestHandler<GetMappingRelationCommand, MappingRelationModel>
{
    private readonly IMappingTablesService _mappingTablesService;

    public GetMappingRelationCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<MappingRelationModel> Handle(GetMappingRelationCommand request, CancellationToken cancellationToken) =>
        _mappingTablesService.GetMappingRelation(request.MappingTableId, request.MappingRelationId, cancellationToken);
}
