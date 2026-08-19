using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Writes the code-list-entry index of the IndexSearch service.
/// </summary>
public interface ICodeListEntryIndexService
{
    Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default);

    Task BuildIndexAsync(CancellationToken cancellationToken = default);

    Task IndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default);

    Task DeIndexAsync(IEnumerable<Guid> codeListEntriesIds, CancellationToken cancellationToken = default);
}
