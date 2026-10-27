using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Business;

/// <summary>
///     Brings one resource in the index up to date, without rebuilding anything around it.
/// </summary>
public interface IIncrementalIndexWriter
{
    /// <summary>
    ///     Re-reads a catalogue resource and replaces its document. A resource that no longer exists is
    ///     removed instead, so a notification that arrives after a delete still leaves the index right.
    /// </summary>
    Task UpsertCatalogResourceAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default);

    Task RemoveCatalogResourceAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces every entry of one concept. The concept is the unit because an entry's ancestor
    ///     codes are derived from its siblings.
    /// </summary>
    Task ReplaceCodeListAsync(Guid conceptId, CancellationToken cancellationToken = default);

    Task RemoveCodeListAsync(Guid conceptId, CancellationToken cancellationToken = default);
}
