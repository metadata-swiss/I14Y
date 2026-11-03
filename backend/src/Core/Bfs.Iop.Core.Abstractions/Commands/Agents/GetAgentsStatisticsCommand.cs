using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record GetAgentsStatisticsCommand : IRequest<IEnumerable<AgentStatisticsResult>>
{ }
