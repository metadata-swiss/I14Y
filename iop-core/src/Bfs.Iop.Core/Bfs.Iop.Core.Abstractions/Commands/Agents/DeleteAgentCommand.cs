using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record DeleteAgentCommand(Guid Id) : IRequest
{ }
