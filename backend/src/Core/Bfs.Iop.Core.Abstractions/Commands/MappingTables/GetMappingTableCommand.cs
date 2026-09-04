using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record GetMappingTableCommand(Guid Id) : IRequest<MappingTableModel>
{ }
