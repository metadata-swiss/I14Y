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
}