using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class CreateMappingTableCommandHandler : IRequestHandler<CreateMappingTableCommand, Guid>
{
    private readonly IMappingTablesService _mappingTablesService;

    public CreateMappingTableCommandHandler(IMappingTablesService mappingTablesService) => 
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<Guid> Handle(CreateMappingTableCommand request, CancellationToken cancellationToken) => 
        _mappingTablesService.AddMappingTable(request.InputModel, cancellationToken);
}
