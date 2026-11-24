using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class ExportMappingTableCommandHandler : IRequestHandler<ExportMappingTableCommand, ExportFile>
{
    private readonly IMappingTablesService _mappingTablesService;

    public ExportMappingTableCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService;

    public async Task<ExportFile> Handle(ExportMappingTableCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFormat is not DataFormat.Json)
        {
            throw new NotSupportedException($"Data format {request.DataFormat} is not supported for mapping table export.");
        }

        var table = await _mappingTablesService.GetMappingTable(request.Id, includeRelations: true, cancellationToken);

        var filename = $"MappingTable_{table.Identifiers.FirstOrDefault(request.Id.ToString())}";

        return IopJsonSerializer.SerializeToFile(filename, table);
    }
}
