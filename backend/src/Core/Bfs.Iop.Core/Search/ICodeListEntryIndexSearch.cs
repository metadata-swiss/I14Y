using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Search;

internal interface ICodeListEntryIndexSearch
{
    Task<PagedResult<CodeListEntrySearchResultEntryModel>> SearchAsync(
        Guid conceptId,
        string language,
        string? query,
        IReadOnlyList<string> filters,
        bool addCodeListEntriesPaths,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CodeListEntryModel>> SearchAllAsync(
        Guid conceptId,
        string language,
        string? query,
        IReadOnlyList<string> filters,
        CancellationToken cancellationToken = default);
}
