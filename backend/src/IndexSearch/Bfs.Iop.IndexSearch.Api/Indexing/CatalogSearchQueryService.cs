using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.IndexSearch.Api.Mappings;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Runs a catalog search and resolves the hits into the public <see cref="SearchResultModel"/>.
/// <para>
/// Exists as a separate, public seam because the vocabulary service it needs is internal to
/// Bfs.Iop.Core and therefore cannot appear on a controller's constructor signature.
/// </para>
/// </summary>
public interface ICatalogSearchQueryService
{
    Task<PagedResult<SearchResultModel>> SearchAsync(
        string? query,
        string? language,
        CatalogSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}

internal sealed class CatalogSearchQueryService : ICatalogSearchQueryService
{
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IAgentsService _agentsService;

    public CatalogSearchQueryService(
        ICatalogIndexService catalogIndexService,
        IVocabulariesService vocabulariesService,
        IAgentsService agentsService)
    {
        _catalogIndexService = catalogIndexService;
        _vocabulariesService = vocabulariesService;
        _agentsService = agentsService;
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
            agents[publisherId] = await _agentsService.GetAgent(publisherId, cancellationToken);
        }

        return new PagedResult<SearchResultModel>
        {
            Page = hits.Page,
            PageSize = hits.PageSize,
            TotalCount = hits.TotalCount,
            Results = hits.Results
                .Select(x => x.MapToSearchResultModel(agents[x.Publisher], _vocabulariesService))
                .ToList(),
        };
    }
}
