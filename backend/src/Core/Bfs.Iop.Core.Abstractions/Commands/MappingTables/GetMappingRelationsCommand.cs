using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record GetMappingRelationsCommand(
    Guid MappingTableId, 
    int? Page, 
    int? PageSize) : IRequest<PagedResult<MappingRelationModel>>
{ }
