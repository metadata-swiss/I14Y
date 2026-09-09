using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Users;

public sealed record GetCurrentUserAgentsCommand : IRequest<IEnumerable<AgentModel>>
{ }
