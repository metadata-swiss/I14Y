using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Data.Indexing;
using Bfs.Iop.IndexSearch.Api.Mappings;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// The Elasticsearch-backed <see cref="ICatalogSearchQueryService"/>: runs the search and resolves
/// the hits into the public <see cref="SearchResultModel"/>.
/// <para>
/// The port itself lives in <c>Bfs.Iop.Search.Abstractions</c> so IOP Core can bind it too — Core's
/// implementation of the same port simply calls this service over HTTP.
/// </para>
/// </summary>
internal sealed class CatalogSearchQueryService : ICatalogSearchQueryService
{
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IVocabularyReader _vocabularies;
    private readonly IAgentReader _agents;
    private readonly ILogger<CatalogSearchQueryService> _logger;

    public CatalogSearchQueryService(
        ICatalogIndexService catalogIndexService,
        IVocabularyReader vocabularies,
        IAgentReader agents,
        ILogger<CatalogSearchQueryService> logger)
    {
        _catalogIndexService = catalogIndexService;
        _vocabularies = vocabularies;
        _agents = agents;
        _logger = logger;
    }

    public async Task<PagedResult<SearchResultModel>> SearchAsync(
        string? query,
        string? language,
        CatalogSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var hits = await _catalogIndexService.SearchAsync(query, language, filter, page, pageSize, cancellationToken);

        // Publishers are resolved once per distinct id rather than once per hit; IOP Core does this
        // per hit, which is an N+1 worth not reproducing.
        var agents = new Dictionary<Guid, AgentModel>();

        foreach (var publisherId in hits.Results.Select(x => x.Publisher).Distinct())
        {
            var agent = await _agents.GetAgent(publisherId, cancellationToken);
            if (agent is not null)
            {
                agents[publisherId] = agent;
            }
        }

        // A hit whose publisher row has gone is a data inconsistency, not a reason to fail the whole
        // page — drop the hit rather than the search. Returning it with a fabricated publisher would
        // be worse: the UI renders that field, so an invented name would read as real data.
        var resolved = hits.Results.Where(x => agents.ContainsKey(x.Publisher)).ToList();

        var orphaned = hits.Results.Count() - resolved.Count;
        if (orphaned > 0)
        {
            // Say so. A dropped hit is invisible to the caller, and the index will keep serving it on
            // every search until the next rebuild removes it or the agent is restored.
            _logger.LogWarning(
                "{Count} search hit(s) were dropped because their publisher could not be resolved. " +
                "The catalog references an agent that no longer exists; the index is stale for those " +
                "resources until the next full rebuild.",
                orphaned);
        }

        return new PagedResult<SearchResultModel>
        {
            Page = hits.Page,
            PageSize = hits.PageSize,
            // Reduced by what was dropped: the UI derives its page count from this, so reporting the
            // unfiltered total would promise rows that no page can ever produce.
            TotalCount = Math.Max(0, hits.TotalCount - orphaned),
            Results = [.. resolved.Select(x => x.MapToSearchResultModel(agents[x.Publisher], _vocabularies))],
        };
    }
}
