using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record CreateMappingTableCommand(MappingTableInputModel InputModel) : IRequest<Guid>
{ }
