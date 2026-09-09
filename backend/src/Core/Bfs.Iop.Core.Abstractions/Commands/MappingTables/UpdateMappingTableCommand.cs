using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record UpdateMappingTableCommand(
    Guid Id, 
    MappingTableInputModel InputModel) : IRequest
{ }
