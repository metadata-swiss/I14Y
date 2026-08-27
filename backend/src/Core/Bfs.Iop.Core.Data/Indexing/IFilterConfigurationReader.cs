using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Reads a concept's annotation-filter configuration from the object store.
/// <para>
/// Code-list search resolves the filters a caller supplies against this configuration; without it
/// every annotation filter silently matches nothing. The read half only — creating and deleting a
/// configuration is authorization-checked and stays in the business layer, whereas reading one is
/// not (see <c>FilterConfigurationFileStorageService</c>: only Upload and Delete check).
/// </para>
/// <para>
/// Exists so the search service can read it without going through MediatR into a handler that is
/// internal to <c>Bfs.Iop.Core</c>.
/// </para>
/// </summary>
public interface IFilterConfigurationReader
{
    /// <summary>
    /// Returns the concept's filter configuration, or <c>null</c> when it has none.
    /// <para>
    /// Null rather than an exception: most concepts legitimately have no configuration, so absence is
    /// an ordinary answer and must be distinguishable from a failure to read the store.
    /// </para>
    /// </summary>
    Task<FilterConfigurationModel?> TryGetAsync(Guid conceptId, CancellationToken cancellationToken = default);
}
