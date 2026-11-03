using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record GetMappingRelationCommand(
    Guid MappingTableId, 
    Guid MappingRelationId) : IRequest<MappingRelationModel>
{ }
