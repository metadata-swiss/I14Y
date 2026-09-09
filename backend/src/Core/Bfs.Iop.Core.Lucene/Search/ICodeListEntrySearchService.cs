using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Lucene.Search;

public interface ICodeListEntrySearchService
{
    Task<PagedResult<CodeListEntrySearchResultEntryModel>> Search(
        Guid conceptId,
        string language,
        string? query,
        List<string> filters,
        bool addCodeListEntriesPaths,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
