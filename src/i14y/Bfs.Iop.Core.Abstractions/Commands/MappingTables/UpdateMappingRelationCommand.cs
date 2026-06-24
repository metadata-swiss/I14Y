using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record UpdateMappingRelationCommand(
    Guid MappingTableId,
    Guid MappingRelationId,
    MappingRelationInputModel InputModel) : IRequest
{ }
