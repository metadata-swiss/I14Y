using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record GetAgentsStatisticsCommand : IRequest<IEnumerable<AgentStatisticsResult>>
{ }
