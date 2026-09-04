using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.AllowActions;

public sealed record GetAllowCreateCommand() : IRequest<IEnumerable<AllowActionResult>>
{ }
