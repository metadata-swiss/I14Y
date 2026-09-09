using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record CreateMappingTableVersionCommand(
    Guid Id, 
    MappingTableInputModel InputModel) : IRequest<Guid>
{ }