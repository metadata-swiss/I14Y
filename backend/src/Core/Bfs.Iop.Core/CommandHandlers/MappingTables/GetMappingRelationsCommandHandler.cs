using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class GetMappingRelationsCommandHandler : IRequestHandler<GetMappingRelationsCommand, PagedResult<MappingRelationModel>>
{
    private readonly IMappingTablesService _mappingTablesService;

    public GetMappingRelationsCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<PagedResult<MappingRelationModel>> Handle(GetMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _mappingTablesService.GetMappingRelations(request.MappingTableId, page, pageSize);
    }
}
