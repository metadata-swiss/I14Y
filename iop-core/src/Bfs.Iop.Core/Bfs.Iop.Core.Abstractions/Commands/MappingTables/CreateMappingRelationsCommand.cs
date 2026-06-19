using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record CreateMappingRelationsCommand(
    Guid MappingTableId, 
    IEnumerable<MappingRelationInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
