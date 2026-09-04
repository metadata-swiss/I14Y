using Bfs.Iop.DataAccess.Contracts;

namespace Bfs.Iop.DataAccess.Relational.Extensions;

public static class AgentsServiceExtensions
{
    public static IReadOnlyDictionary<string, Guid> GetAgentsIdentifiersIdsDictionary(
        this IAgentsService agentsService,
        IEnumerable<string> agentIdentifiers)
    {
        ArgumentNullException.ThrowIfNull(agentsService, nameof(agentsService));
        ArgumentNullException.ThrowIfNull(agentIdentifiers, nameof(agentIdentifiers));

        // this operation cannot be async because DbContext does not like concurrent access

        return agentIdentifiers
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => agentsService.GetAgentId(x.Key, default).GetAwaiter().GetResult())
            .AsReadOnly();
    }
}
