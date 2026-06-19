using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record DeleteAllMappingRelationsCommand(Guid MappingTableId) : IRequest
{ }
