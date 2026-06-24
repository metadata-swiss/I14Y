using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Serialization.Csv;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class ImportMappingRelationsCommandHandler : IRequestHandler<ImportMappingRelationsCommand>
{
    private readonly IMappingTablesService _mappingTablesService;

    public ImportMappingRelationsCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public async Task Handle(ImportMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        // Verify if there are no relations
        var pagedResult = await _mappingTablesService.GetMappingRelations(request.MappingTableId, page: 1, pageSize: 1, cancellationToken);

        if (pagedResult.TotalCount > 0)
        {
            throw new MethodNotAllowedException("The mapping table contains relations. Please delete them and try again.");
        }

        var relations = request.DataFormat switch
        {
            MappingRelationsDataFormat.Json => IopJsonSerializer.DeserializeStreamData<IEnumerable<MappingRelationInputModel>>(request.Data),
            MappingRelationsDataFormat.Csv => MappingRelationsCsvSerializer.DeserializeStreamData(request.Data),
            _ => throw new NotSupportedException($"The format '{request.DataFormat}' is not allowed.")
        };

        await _mappingTablesService.AddRelations(request.MappingTableId, relations, cancellationToken);

        return;
    }
}
