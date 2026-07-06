using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record DeleteMappingRelationCommand(Guid MappingTableId, Guid MappingRelationId) : IRequest
{ }
