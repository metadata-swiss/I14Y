using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingRelationsCommandHandler : IRequestHandler<CreateMappingRelationsCommand, IEnumerable<Guid>>
{
    private readonly IMappingTablesService _mappingTablesService;

    public CreateMappingRelationsCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService
            ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<IEnumerable<Guid>> Handle(CreateMappingRelationsCommand request, CancellationToken cancellationToken) =>
        _mappingTablesService.AddRelations(request.MappingTableId, request.InputModels, cancellationToken);
}
