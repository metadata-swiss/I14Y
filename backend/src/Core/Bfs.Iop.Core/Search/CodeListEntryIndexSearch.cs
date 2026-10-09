using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

using Client = Bfs.Iop.IndexSearch.Contracts.Search;
using ApiClient = Bfs.Iop.IndexSearch.ApiClient;
using IIndexSearchApiClient = Bfs.Iop.IndexSearch.ApiClient.IIndexSearchApiClient;

namespace Bfs.Iop.Core.Search;

internal sealed class CodeListEntryIndexSearch : ICodeListEntryIndexSearch
{
    private readonly IIndexSearchApiClient _search;
    private readonly IIopConceptsService _conceptsService;
    private readonly IMediator _mediator;

    public CodeListEntryIndexSearch(
        IIndexSearchApiClient search,
        IIopConceptsService conceptsService,
        IMediator mediator)
    {
        _search = search ?? throw new ArgumentNullException(nameof(search));
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
        catch
        {
            // A concept with no filter configuration is ordinary rather than an error; the Lucene path
            // fell back to an empty one the same way.
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
    ///     The breadcrumb for each hit, root first and ending at the entry itself. The index already
    ///     carries every ancestor code, so this is one query for the whole page — the Lucene path
    ///     climbed the tree instead, one database round trip per ancestor per hit.
    /// </summary>
    private async Task<Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>> PathsAsync(
        Guid conceptId,
        IReadOnlyCollection<Client.CodeListSearchHit> hits,
        Dictionary<Guid, CodeListEntryModel> entries,
        CancellationToken cancellationToken)
    {
        // Ancestors are indexed nearest-parent first, and a path reads from the root down.
        var ancestorsOf = hits.ToDictionary(
            x => x.Id,
            x => (IReadOnlyList<string>)[.. (x.AncestorCodes ?? []).Reverse()]);

        var codes = ancestorsOf.Values.SelectMany(x => x).Distinct(StringComparer.Ordinal).ToArray();

        var ancestors = codes.Length == 0
            ? []
            : (await _conceptsService.GetCodeListEntriesByCodes(conceptId, codes, cancellationToken))
                .GroupBy(x => x.Code, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

        return hits
            .Where(x => entries.ContainsKey(x.Id))
            .ToDictionary(
                x => x.Id,
                x => (IEnumerable<CodeListEntrySearchResultPathModel>)
                [
                    .. ancestorsOf[x.Id]
                        .Where(ancestors.ContainsKey)
                        .Select(code => ToPath(ancestors[code]))
                        .Append(ToPath(entries[x.Id])),
                ]);
    }

    private static CodeListEntrySearchResultPathModel ToPath(CodeListEntryModel entry) => new()
    {
        Code = entry.Code,
        Name = entry.Name,
        ParentCode = entry.ParentCode,
    };
}
