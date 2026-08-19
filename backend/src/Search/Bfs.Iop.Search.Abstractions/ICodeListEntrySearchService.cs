using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;

namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Queries the code-list-entry index of the IndexSearch service.
/// </summary>
public interface ICodeListEntrySearchService
{
    Task<PagedResult<CodeListEntrySearchResultEntryModel>> SearchAsync(
        Guid conceptId,
        string language,
        string? query,
        List<string> filters,
        bool addCodeListEntriesPaths,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
