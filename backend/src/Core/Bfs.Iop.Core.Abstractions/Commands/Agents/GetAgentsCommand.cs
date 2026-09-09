using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

public sealed record GetAgentsCommand(
    string? Identifier,
    string? Uid, 
    int? Page, 
    int? PageSize) : IRequest<PagedResult<AgentModel>>
{ }
