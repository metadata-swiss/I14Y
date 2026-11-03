using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.AllowActions;

public sealed record GetAllowCreateCommand() : IRequest<IEnumerable<AllowActionResult>>
{ }
