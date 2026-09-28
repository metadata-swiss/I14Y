using System.Runtime.CompilerServices;
using Bfs.Iop.IndexSearch.Contracts.Search;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListSearchEngine : ICodeListSearchEngine
{
    /// <summary>How long the frozen view stays alive between pages.</summary>
    private static readonly TimeSpan KeepAlive = TimeSpan.FromMinutes(2);

    /// <summary>Entries per round trip while walking.</summary>
    private const int BatchSize = 1_000;

    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;

    public ElasticsearchCodeListSearchEngine(ElasticsearchSearchExecutor executor, IndexNames names)
    {
        _executor = executor;
        _names = names;
    }

    public async Task<PagedResult<CodeListSearchHit>> SearchAsync(
        Guid conceptId,
        string? query,
        string language,
        CodeListSearchFilter? filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (from, size) = Paging.ToWindow(page, pageSize);

        var body = CodeListQueryBuilder.BuildSearchBody(conceptId, query, language, filter, from, size);

        using var response = await _executor.SearchAsync(_names.CodeList, body, cancellationToken);

        return CodeListResponseReader.ReadSearch(response.RootElement, page, pageSize);
    }

    /// <summary>
    ///     Every matching entry, however many there are.
    ///     <para>
    ///         Page numbers cannot do this: <c>from</c> is capped at the index result window, so a walk
    ///         by page silently runs out at 10 000 - and the largest code lists here hold well over
    ///         twice that. A point in time with <c>search_after</c> has no ceiling and, because the
    ///         view is frozen, a nightly reindex swapping the alias mid-walk cannot make it repeat or
    ///         skip entries.
    ///     </para>
    /// </summary>
    public async IAsyncEnumerable<CodeListSearchHit> StreamAllAsync(
        Guid conceptId,
        string? query,
        string language,
        CodeListSearchFilter? filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var pit = await _executor.OpenPointInTimeAsync(_names.CodeList, KeepAlive, cancellationToken);

        try
        {
            IReadOnlyList<object>? cursor = null;

            while (true)
            {
                var body = CodeListQueryBuilder.BuildPointInTimeBody(
                    conceptId, query, language, filter, pit, KeepAlive, BatchSize, cursor);

                using var response = await _executor.SearchPointInTimeAsync(body, cancellationToken);

                pit = CodeListResponseReader.ReadPointInTimeId(response.RootElement) ?? pit;

                var page = CodeListResponseReader.ReadPage(response.RootElement);

                if (page.Count == 0)
                {
                    yield break;
                }

                foreach (var (hit, _) in page)
                {
                    yield return hit;
                }

                cursor = page[^1].Cursor;

                // A page without a cursor cannot be continued, and continuing from the same place
                // would loop forever. Stopping is the only safe move, and it means a shorter answer,
                // so the caller is told rather than quietly short-changed.
                if (cursor.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Elasticsearch returned code list hits without a sort cursor, so the remaining "
                        + "entries cannot be read. The index may have been rebuilt mid-export.");
                }
            }
        }
        finally
        {
            await _executor.ClosePointInTimeAsync(pit, CancellationToken.None);
        }
    }
}
