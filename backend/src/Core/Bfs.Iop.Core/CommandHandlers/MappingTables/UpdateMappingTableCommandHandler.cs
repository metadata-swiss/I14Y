using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class UpdateMappingTableCommandHandler : IRequestHandler<UpdateMappingTableCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public UpdateMappingTableCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task Handle(UpdateMappingTableCommand request, CancellationToken cancellationToken)
    {
        return _mappingTablesService.UpdateMappingTable(request.Id, request.InputModel, cancellationToken);
    }
}
