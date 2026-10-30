using System.Runtime.CompilerServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogSearchEngine : ICatalogSearchEngine
{
    private static readonly TimeSpan KeepAlive = TimeSpan.FromMinutes(2);

    private const int BatchSize = 1_000;

    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;
    private readonly CatalogScoring _scoring;

    public ElasticsearchCatalogSearchEngine(
        ElasticsearchSearchExecutor executor,
        IndexNames names,
        IOptions<SearchScoringOptions> scoring)
    {
        _executor = executor;
        _names = names;
        _scoring = scoring.Value.Catalog;
    }

    public async Task<PagedResult<CatalogSearchHit>> SearchAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (from, size) = Paging.ToWindow(page, pageSize);

        var body = CatalogQueryBuilder.BuildSearchBody(query, languages, filter, caller, _scoring, from, size);

        using var response = await _executor.SearchAsync(_names.Catalog, body, cancellationToken);

        return CatalogResponseReader.ReadSearch(response.RootElement, page, pageSize);
    }

    /// <summary>
    ///     Every match, however many there are, from a frozen view of the index.
    /// </summary>
    /// <remarks>
    ///     Walking by page number stops at the result window and costs Core a request per page. A
    ///     point in time has no ceiling, and because the view is frozen a rebuild swapping the alias
    ///     mid-walk cannot make it repeat or skip resources.
    /// </remarks>
    public async IAsyncEnumerable<CatalogSearchHit> StreamAllAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var pit = await _executor.OpenPointInTimeAsync(_names.Catalog, KeepAlive, cancellationToken);

        try
        {
            IReadOnlyList<object>? cursor = null;

            while (true)
            {
                var body = CatalogQueryBuilder.BuildPointInTimeBody(
                    query, languages, filter, caller, _scoring, pit, KeepAlive, BatchSize, cursor);

                using var response = await _executor.SearchPointInTimeAsync(body, cancellationToken);

                pit = CatalogResponseReader.ReadPointInTimeId(response.RootElement) ?? pit;

                var page = CatalogResponseReader.ReadPage(response.RootElement);

                if (page.Count == 0)
                {
                    yield break;
                }

                foreach (var (hit, _) in page)
                {
                    yield return hit;
                }

                cursor = page[^1].Cursor;

                if (cursor.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Elasticsearch returned catalogue hits without a sort cursor, so the remaining "
                        + "resources cannot be read. The index may have been rebuilt mid-walk.");
                }
            }
        }
        finally
        {
            await _executor.ClosePointInTimeAsync(pit, CancellationToken.None);
        }
    }

    public async Task<CatalogFacetCounts> CountAsync(
        string? query,
        IReadOnlyList<string> languages,
        CatalogSearchFilter? filter,
        SearchCaller caller,
        CancellationToken cancellationToken = default)
    {
        var body = CatalogQueryBuilder.BuildCountBody(query, languages, filter, caller, _scoring);

        using var response = await _executor.SearchAsync(_names.Catalog, body, cancellationToken);

        return CatalogResponseReader.ReadFacets(response.RootElement);
    }
}
