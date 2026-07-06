using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record UpdateMappingTableCommand(
    Guid Id, 
    MappingTableInputModel InputModel) : IRequest
{ }
