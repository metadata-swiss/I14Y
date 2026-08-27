using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Vocabularies;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Data.Indexing;

/// <inheritdoc cref="IAgentReader"/>
/// <remarks>
/// The <c>Include</c> set matches <c>AgentsService</c>'s "all" level, because
/// <c>MapToAgentModel</c> reads every one of those navigations. Dropping one does not fail — it
/// produces a publisher with an empty contact point or no sub-agents, silently.
/// </remarks>
internal sealed class AgentReader : IAgentReader
{
    private readonly IopDbContext _dbContext;
    private readonly IVocabularyReader _vocabularies;

    public AgentReader(IopDbContext dbContext, IVocabularyReader vocabularies)
    {
        _dbContext = dbContext;
        _vocabularies = vocabularies;
    }

    public async Task<AgentModel?> GetAgent(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToAgentModel(_vocabularies);
    }

    public async Task<IReadOnlyList<AgentModel>> GetAgents(
        IEnumerable<string> identifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifiers);

        var wanted = identifiers.ToArray();
        if (wanted.Length == 0)
        {
            return [];
        }

        var entities = await Query()
            .Where(x => wanted.Contains(x.Identifier))
            .ToListAsync(cancellationToken);

        return [.. entities.Select(x => x.MapToAgentModel(_vocabularies))];
    }

    private IQueryable<Agent> Query() =>
        _dbContext.Agents
            .AsNoTracking()
            .Include(d => d.ContactPoint)
            .Include(d => d.Description)
            .Include(d => d.Images)
            .Include(d => d.Name)
            .Include(d => d.SubAgents)
                .ThenInclude(s => s.SubAgent);
}
