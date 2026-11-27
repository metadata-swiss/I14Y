using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record ExportMappingTableCommand(Guid Id, DataFormat DataFormat) : IRequest<ExportFile>
{ }
