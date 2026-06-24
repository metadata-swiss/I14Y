using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Users;

public sealed record GetCurrentUserAgentsCommand : IRequest<IEnumerable<AgentModel>>
{ }
