using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Writes and queries the catalog index of the IndexSearch service.
/// <para>
/// These contracts belong to the IndexSearch service. Nothing here may depend on a concrete search
/// engine: Bfs.Iop.Core depends on this project, and so does the engine that implements it.
/// </para>
/// </summary>
public interface ICatalogIndexService
{
    /// <summary>
    /// Creates the index and its mapping if missing. When <paramref name="recreate"/> is true the
    /// existing index is dropped first, which discards every document.
    /// </summary>
    Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a batch of already-projected entries.
    /// <para>
    /// One method for all five resource kinds: <see cref="CatalogIndexEntry.Type"/> carries the
    /// discriminator, so the caller no longer picks an overload by model type — a choice that used to
    /// be made by hand and could route a dataset to the wrong document shape.
    /// </para>
    /// <para>
    /// An entry whose <see cref="CatalogIndexEntry.HasStructure"/> is null means "keep whatever is
    /// already indexed" — the flag comes from the object store, not the database, so a caller that
    /// does not know it must not be able to clear it by omission.
    /// </para>
    /// </summary>
    Task UpdateIndexAsync(IEnumerable<CatalogIndexEntry> entries, CancellationToken cancellationToken = default);

    Task DeIndexAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<PagedResult<CatalogSearchResultEntry>> SearchAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<CatalogSearchCountResultEntry>> SearchCountAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        CancellationToken cancellationToken = default);
}
