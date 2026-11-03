using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record UpdateAgentCommand(Guid Id, AgentInputModel UpdateModel) : IRequest
{ }
