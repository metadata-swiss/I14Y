using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingTableVersionCommandHandler : IRequestHandler<CreateMappingTableVersionCommand, Guid>
{
    private readonly IMappingTablesService _mappingTablesService;

    public CreateMappingTableVersionCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService;

    public Task<Guid> Handle(CreateMappingTableVersionCommand request, CancellationToken cancellationToken) => 
        _mappingTablesService.AddMappingTableVersion(request.Id, request.InputModel, cancellationToken);
}
