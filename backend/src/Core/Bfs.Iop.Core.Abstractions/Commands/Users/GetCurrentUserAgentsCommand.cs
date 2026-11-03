using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Users;

public sealed record GetCurrentUserAgentsCommand : IRequest<IEnumerable<AgentModel>>
{ }
