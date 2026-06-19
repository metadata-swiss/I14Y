using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.AllowActions;

public sealed record GetAllowCreateCommand() : IRequest<IEnumerable<AllowActionResult>>
{ }
