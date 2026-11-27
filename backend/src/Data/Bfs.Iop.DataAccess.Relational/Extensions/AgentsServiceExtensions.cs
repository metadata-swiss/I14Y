using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;

namespace Bfs.Iop.DataAccess.Relational.Extensions;

public static class AgentsServiceExtensions
{
    public static async Task<IReadOnlyDictionary<string, Guid>> GetAgentsIdentifiersIdsDictionary(
        this IAgentsService agentsService,
        IEnumerable<string> agentIdentifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(agentsService, nameof(agentsService));
        ArgumentNullException.ThrowIfNull(agentIdentifiers, nameof(agentIdentifiers));

        var result = new Dictionary<string, Guid>();

        // Sequential awaits: DbContext does not support concurrent access.
        foreach (var identifier in agentIdentifiers.Distinct())
        {
            var id = await agentsService.TryGetAgentId(identifier, cancellationToken);

            if (id.HasValue)
            {
                result.Add(identifier, id.Value);
            }
        }

        return result.AsReadOnly();
    }

    public static async Task<Guid?> TryGetAgentId(this IAgentsService agentsService, string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(agentsService, nameof(agentsService));
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        try
        {
            return await agentsService.GetAgentId(identifier, cancellationToken);
        }
        catch (NotFoundException)
        {
            // do nothing, return null.
        }

        return null;
    }
}