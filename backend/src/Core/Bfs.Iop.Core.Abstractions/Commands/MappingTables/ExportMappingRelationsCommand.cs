using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record ExportMappingRelationsCommand(
    Guid MappingTableId,
    MappingRelationsDataFormat DataFormat) : IRequest<ExportFile>
{ }
