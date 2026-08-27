using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Resolves publishers for search results.
/// <para>
/// The index stores a publisher's id and identifier, not their name — so a search result and the
/// publishers facet both have to look the agent up to label it. This is the read-only slice of that
/// lookup, extracted so the IndexSearch service can resolve publishers without the business layer.
/// </para>
/// <para>
/// <b>Unauthorized by design</b>, matching the behaviour it replaces: <c>AgentsService.GetAgent</c>
/// and <c>GetAgents</c> apply no user predicate either. An agent is public reference data, and the
/// resources that reference it are filtered separately at query time.
/// </para>
/// </summary>
public interface IAgentReader
{
    /// <summary>Resolves one publisher by id. Null when the agent no longer exists.</summary>
    Task<AgentModel?> GetAgent(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves publishers by identifier, for the publishers facet.
    /// <para>
    /// Matching is case-sensitive, because the underlying column comparison is. That is why the index
    /// stores the publisher identifier twice — lowercased for term matching and case-preserved for
    /// the facet — and why the facet must pass the case-preserved form here.
    /// </para>
    /// </summary>
    Task<IReadOnlyList<AgentModel>> GetAgents(IEnumerable<string> identifiers, CancellationToken cancellationToken = default);
}
