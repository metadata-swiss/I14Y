using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.IndexSearch.ApiClient;

/// <summary>
/// Read side of the IndexSearch API: catalog search, facet counts, and code-list search.
/// <para>
/// Separate from <see cref="IIndexSearchApiClient"/> because the two directions authenticate
/// differently. Reads carry the **caller's bearer token** — results are user-scoped, and the query
/// builder derives the caller's role and agencies from that token. Writes carry a shared secret,
/// because an index writer has no user. Two interfaces make it impossible to send a read through the
/// write client and silently get public-only results.
/// </para>
/// </summary>
public interface IIndexSearchSearchClient
{
    /// <summary>
    /// Searches the catalog. Paging is read from the response headers, so callers get a
    /// <see cref="PagedResult{T}"/> rather than having to know the header protocol.
    /// </summary>
    /// <param name="query">Free-text query; null or empty matches everything.</param>
    /// <param name="language">Language to search and return; null searches all configured languages.</param>
    /// <param name="filter">Facet selections.</param>
    /// <param name="page">Page number. Omit together with <paramref name="pageSize"/> for everything.</param>
    /// <param name="pageSize">Page size. Omit together with <paramref name="page"/> for everything.</param>
    /// <param name="cancellationToken"></param>
    Task<PagedResult<SearchResultModel>> SearchAsync(
        string? query,
        string? language,
        CatalogSearchFilter? filter,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the facet counts for the same query.</summary>
    Task<SearchCountResultModel> SearchCountAsync(
        string? query,
        string? language,
        CatalogSearchFilter? filter,
        CancellationToken cancellationToken = default);

    /// <summary>Searches the entries of one code list.</summary>
    Task<PagedResult<CodeListEntrySearchResultEntryModel>> SearchCodeListEntriesAsync(
        Guid conceptId,
        string language,
        string? query,
        IEnumerable<string>? filters,
        bool addCodeListEntriesPaths,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default);
}
