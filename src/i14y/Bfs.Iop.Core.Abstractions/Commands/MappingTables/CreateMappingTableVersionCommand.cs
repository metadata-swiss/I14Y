using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record CreateMappingTableVersionCommand(
    Guid Id, 
    MappingTableInputModel InputModel) : IRequest<Guid>
{ }