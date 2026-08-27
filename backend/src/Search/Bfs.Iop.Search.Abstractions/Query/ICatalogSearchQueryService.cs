using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Catalog search, already resolved into the public <see cref="SearchResultModel"/>.
/// <para>
/// Sits alongside <see cref="ICodeListEntrySearchService"/> and has the same shape: it returns a
/// finished Core.Abstractions model rather than a raw index entry. That is the distinction between
/// these two ports and <see cref="ICatalogIndexService"/>, which speaks in index entries — these are
/// what a caller consumes, that is what an engine produces.
/// </para>
/// <para>
/// Implemented once, inside <c>Bfs.Iop.IndexSearch.Api</c>, against Elasticsearch. It is the seam
/// between that service's <c>SearchController</c> and its engine, and nothing outside that process
/// implements it — IOP Core reaches search through <c>IIndexSearchSearchClient</c> instead.
/// </para>
/// <para>
/// So a parameter added here does <b>not</b> reach Core. The client's signature is the wire contract
/// Core sees, and the two are held together by <c>SearchQueryContractTests</c>, which reflects over
/// the controllers on both sides.
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

/// <summary>
/// Facet counts for the catalog filters. See <see cref="ICatalogSearchQueryService"/> for why this
/// lives here rather than in either host.
/// <para>
/// Deliberately a sibling of the search port rather than a method on it: a count labels a result
/// list, so the two must always be answered by the same engine. Registering them together is what
/// makes that true by construction.
/// </para>
/// </summary>
public interface ICatalogSearchCountQueryService
{
    Task<SearchCountResultModel> SearchCountAsync(
        string? query,
        string? language,
        CatalogSearchFilter filter,
        CancellationToken cancellationToken);
}
