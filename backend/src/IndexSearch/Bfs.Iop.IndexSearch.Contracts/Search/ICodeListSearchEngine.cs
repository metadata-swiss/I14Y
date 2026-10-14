using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public interface ICodeListSearchEngine
{
    Task<PagedResult<CodeListSearchHit>> SearchAsync(
        Guid conceptId,
        string? query,
        string language,
        CodeListSearchFilter? filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Every matching entry, for an export. Not bounded by the index result window, unlike
    ///     <see cref="SearchAsync" />, which pages by number and therefore stops at it.
    /// </summary>
    IAsyncEnumerable<CodeListSearchHit> StreamAllAsync(
        Guid conceptId,
        string? query,
        string language,
        CodeListSearchFilter? filter,
        CancellationToken cancellationToken = default);
}
