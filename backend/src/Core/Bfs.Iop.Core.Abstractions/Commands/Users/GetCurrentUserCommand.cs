using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Users;

public sealed record GetCurrentUserCommand : IRequest<UserModel>
{ }
