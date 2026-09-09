using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface IAgentsService : IAuthorizedEntityService
{
    Task<AgentModel> GetAgent(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<AgentModel>> GetAgents(
        string? identifier,
        string? uid, 
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AgentModel>> GetAgents(IEnumerable<string> identifiers, CancellationToken cancellationToken = default);

    Task<IEnumerable<AgentModel>> GetAgents(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<IEnumerable<AgentModel>> GetAgentParentAgents(Guid id, CancellationToken cancellationToken= default);

    Task<Guid> GetAgentId(string identifier, CancellationToken cancellationToken);

    Task<Guid> AddAgent(AgentInputModel inputModel, CancellationToken cancellationToken = default);

    Task UpdateAgent(Guid id, AgentInputModel updateModel, CancellationToken cancellationToken = default);

    Task DeleteAgent(Guid id, CancellationToken cancellationToken = default);
}
