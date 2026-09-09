using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record GetAgentParentAgentsCommand(Guid Id) : IRequest<IEnumerable<AgentModel>>
{ }
