using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record DeleteMappingTableCommand(Guid Id) : IRequest
{ }
