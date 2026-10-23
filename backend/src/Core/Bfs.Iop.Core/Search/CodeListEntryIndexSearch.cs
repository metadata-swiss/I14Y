using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

using Client = Bfs.Iop.IndexSearch.Contracts.Search;
using ApiClient = Bfs.Iop.IndexSearch.ApiClient;
using IIndexSearchApiClient = Bfs.Iop.IndexSearch.ApiClient.IIndexSearchApiClient;

namespace Bfs.Iop.Core.Search;

internal sealed class CodeListEntryIndexSearch : ICodeListEntryIndexSearch
{
    private readonly IIndexSearchApiClient _search;
    private readonly IIopConceptsService _conceptsService;
    private readonly IMediator _mediator;
    private readonly ILogger<CodeListEntryIndexSearch> _logger;

    public CodeListEntryIndexSearch(
        IIndexSearchApiClient search,
        IIopConceptsService conceptsService,
        IMediator mediator,
        ILogger<CodeListEntryIndexSearch> logger)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<CodeListEntrySearchResultEntryModel>> SearchAsync(
        Guid conceptId,
        string language,
        string? query,
        IReadOnlyList<string> filters,
        bool addCodeListEntriesPaths,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = await BuildFilterAsync(conceptId, filters, cancellationToken);

        List<Client.CodeListSearchHit> hits;
        int resolvedPage;
        int resolvedPageSize;
        int totalCount;

        if (page is int requestedPage && pageSize is int requestedPageSize)
        {
            var result = await QueryAsync(
                conceptId, language, query, filter, requestedPage, requestedPageSize, cancellationToken);

            hits = (result.Results ?? []).ToList();
            resolvedPage = result.Page ?? requestedPage;
            resolvedPageSize = result.PageSize ?? requestedPageSize;
            totalCount = result.TotalCount ?? 0;
        }
        else
        {
            hits = [.. await AllHitsAsync(conceptId, language, query, filter, cancellationToken)];
            resolvedPage = 1;
            resolvedPageSize = hits.Count;
            totalCount = hits.Count;
        }

        var entries = await EntriesAsync(hits, cancellationToken);

        var paths = addCodeListEntriesPaths
            ? await PathsAsync(conceptId, hits, entries, cancellationToken)
            : [];

        return new PagedResult<CodeListEntrySearchResultEntryModel>
        {
            Page = resolvedPage,
            PageSize = resolvedPageSize,
            TotalCount = totalCount,

            // Hit order is relevance order, so it is the one thing that must survive the round trip
            // through the database.
            Results =
            [
                .. hits
                    .Where(x => entries.ContainsKey(x.Id))
                    .Select(x => new CodeListEntrySearchResultEntryModel
                    {
                        Entry = entries[x.Id],
                        Score = x.Score,
                        Path = paths.TryGetValue(x.Id, out var path) ? path : [],
                    }),
            ],
        };
    }

    public async Task<IReadOnlyList<CodeListEntryModel>> SearchAllAsync(
        Guid conceptId,
        string language,
        string? query,
        IReadOnlyList<string> filters,
        CancellationToken cancellationToken = default)
    {
        var filter = await BuildFilterAsync(conceptId, filters, cancellationToken);

        var hits = await AllHitsAsync(conceptId, language, query, filter, cancellationToken);

        var entries = await EntriesAsync(hits, cancellationToken);

        return [.. hits.Where(x => entries.ContainsKey(x.Id)).Select(x => entries[x.Id])];
    }

    private async Task<IReadOnlyList<Client.CodeListSearchHit>> AllHitsAsync(
        Guid conceptId,
        string language,
        string? query,
        Client.CodeListSearchFilter filter,
        CancellationToken cancellationToken)
    {
        var response = await _search.PostSearchCodelistsAllByBodyAsync(
            new Client.CodeListSearchRequest
            {
                ConceptId = conceptId,
                Query = query,
                Language = language,
                Filter = filter,
            },
            cancellationToken);

        return [.. response.Result ?? []];
    }

    private async Task<ApiClient.CodeListSearchHitPagedResult> QueryAsync(
        Guid conceptId,
        string language,
        string? query,
        Client.CodeListSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var response = await _search.PostSearchCodelistsByBodyAsync(
            new Client.CodeListSearchRequest
            {
                ConceptId = conceptId,
                Query = query,
                Language = language,
                Filter = filter,
                Page = page,
                PageSize = pageSize,
            },
            cancellationToken);

        return response.Result;
    }

    private async Task<Client.CodeListSearchFilter> BuildFilterAsync(
        Guid conceptId,
        IReadOnlyList<string> filters,
        CancellationToken cancellationToken)
    {
        // The only authorization there is, anywhere, for code list entries. The index carries no
        // publication level, so the search service cannot filter by caller the way the catalogue
        // does - which is also why that service must stay internal-only. If this check is ever
        // removed or bypassed, any caller reads any concept's entries.
        _ = await _conceptsService.GetIopConcept(conceptId, includeCodeListEntries: false, cancellationToken);

        return filters.MapToIndexSearchFilter(await FilterConfigurationAsync(conceptId, cancellationToken));
    }

    private async Task<FilterConfigurationModel> FilterConfigurationAsync(
        Guid conceptId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _mediator.Send(new GetFilterConfigurationCommand(conceptId), cancellationToken);
        }
        catch (NotFoundException)
        {
            // A concept with no filter configuration is ordinary rather than an error. Only this one:
            // catching everything turned a cancelled request or an unreachable blob store into a
            // search that quietly ignored the caller's filters.
            return new FilterConfigurationModel { Filters = [] };
        }
    }

    /// <summary>
    ///     The entries themselves, read from the database. The index holds enough to match on but not
    ///     everything the API returns — annotation identities among it — so the hits decide which
    ///     entries and in what order, and the database says what they are.
    /// </summary>
    private async Task<Dictionary<Guid, CodeListEntryModel>> EntriesAsync(
        IReadOnlyCollection<Client.CodeListSearchHit> hits,
        CancellationToken cancellationToken)
    {
        if (hits.Count == 0)
        {
            return [];
        }

        var entries = await _conceptsService.GetCodeListEntriesByIds(
            [.. hits.Select(x => x.Id).Distinct()],
            cancellationToken);

        return entries.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
    }

    /// <summary>
    ///     The breadcrumb for each hit, root first and ending at the entry itself. The index carries
    ///     most ancestry, but the database remains the source of truth when a stale or incomplete index
    ///     entry is missing one or more ancestors.
    /// </summary>
    private async Task<Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>> PathsAsync(
        Guid conceptId,
        IReadOnlyCollection<Client.CodeListSearchHit> hits,
        Dictionary<Guid, CodeListEntryModel> entries,
        CancellationToken cancellationToken)
    {
        var ancestors = entries.Values
            .GroupBy(x => x.Code, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        var codes = hits.SelectMany(x => x.AncestorCodes ?? [])
            .Distinct(StringComparer.Ordinal)
            .Where(code => !ancestors.ContainsKey(code))
            .ToArray();

        if (codes.Length > 0)
        {
            try
            {
                foreach (var ancestor in await _conceptsService.GetCodeListEntriesByCodes(conceptId, codes, cancellationToken))
                {
                    ancestors[ancestor.Code] = ancestor;
                }
            }
            catch (KeyNotFoundException exception)
            {
                // The bulk lookup throws when an indexed code has since been removed.
                _logger.LogWarning(exception,
                    "Indexed ancestors for concept {ConceptId} no longer exist; resolving database parent links instead.",
                    conceptId);
            }
        }

        var paths = new Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>();
        var requestedParentCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var hit in hits.Where(x => entries.ContainsKey(x.Id)))
        {
            paths[hit.Id] = await BuildPathFromParentCodeAsync(
                conceptId, entries[hit.Id], ancestors, requestedParentCodes, cancellationToken);
        }

        return paths;
    }

    private async Task<List<CodeListEntrySearchResultPathModel>> BuildPathFromParentCodeAsync(
        Guid conceptId,
        CodeListEntryModel entry,
        Dictionary<string, CodeListEntryModel> ancestors,
        HashSet<string> requestedParentCodes,
        CancellationToken cancellationToken)
    {
        var path = new List<CodeListEntrySearchResultPathModel>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = entry;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            seen.Add(current.Code);
            path.Add(ToPath(current));

            if (string.IsNullOrWhiteSpace(current.ParentCode))
            {
                break;
            }

            if (seen.Contains(current.ParentCode))
            {
                _logger.LogWarning(
                    "Invalid ancestor chain for code {Code} in concept {ConceptId}: cycle at {ParentCode}.",
                    entry.Code, conceptId, current.ParentCode);
                break;
            }

            if (!ancestors.TryGetValue(current.ParentCode, out var parent))
            {
                parent = await ResolveParentEntryAsync(conceptId, current.ParentCode, ancestors, requestedParentCodes, cancellationToken);
            }

            if (parent is null)
            {
                break;
            }

            current = parent;
        }

        // A truncated path must not retain the unresolved or cyclic parent link.
        path[^1] = path[^1] with { ParentCode = null };
        path.Reverse();
        return path;
    }

    private async Task<CodeListEntryModel?> ResolveParentEntryAsync(
        Guid conceptId,
        string parentCode,
        Dictionary<string, CodeListEntryModel> ancestors,
        HashSet<string> requestedParentCodes,
        CancellationToken cancellationToken)
    {
        if (ancestors.ContainsKey(parentCode))
        {
            return ancestors[parentCode];
        }

        if (!requestedParentCodes.Add(parentCode))
        {
            return null;
        }

        try
        {
            var parent = await _conceptsService.GetCodeListEntryByCode(conceptId, parentCode, cancellationToken);
            ancestors[parent.Code] = parent;
            return parent;
        }
        catch (NotFoundException exception)
        {
            _logger.LogWarning(exception,
                "Parent code {ParentCode} is missing in concept {ConceptId}; returning a truncated breadcrumb.",
                parentCode, conceptId);
            return null;
        }
    }

    private static CodeListEntrySearchResultPathModel ToPath(CodeListEntryModel entry) => new()
    {
        Code = entry.Code,
        Name = entry.Name,
        ParentCode = entry.ParentCode,
    };
}
