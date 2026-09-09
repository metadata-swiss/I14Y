using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.Core.Serialization.Csv;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class ExportMappingRelationsCommandHandler : IRequestHandler<ExportMappingRelationsCommand, ExportFile>
{
    private readonly IMappingTablesService _mappingTablesService;

    public ExportMappingRelationsCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public async Task<ExportFile> Handle(ExportMappingRelationsCommand request, CancellationToken cancellationToken)
    {
        var table = await _mappingTablesService.GetMappingTable(request.MappingTableId, cancellationToken);

        var relations = await _mappingTablesService.GetMappingRelations(
            request.MappingTableId,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        var filename = $"MappingRelations_{table.Identifiers.First()}-{table.Version}";

        return request.DataFormat switch
        {
            MappingRelationsDataFormat.Json => IopJsonSerializer.SerializeToFile(filename, relations.Results),
            MappingRelationsDataFormat.Csv => MappingRelationsCsvSerializer.SerializeToFile(filename, relations.Results),
            _ => throw new NotSupportedException($"The format '{request.DataFormat}' is not supported.")
        };
    }
}
