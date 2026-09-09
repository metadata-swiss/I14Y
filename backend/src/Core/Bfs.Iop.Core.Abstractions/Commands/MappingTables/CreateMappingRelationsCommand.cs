using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record CreateMappingRelationsCommand(
    Guid MappingTableId, 
    IEnumerable<MappingRelationInputModel> InputModels) : IRequest<IEnumerable<Guid>>
{ }
