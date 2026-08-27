using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;

namespace Bfs.Iop.IndexSearch.ApiClient;

/// <summary>Current state of the IndexSearch service's indexes and ingest queue.</summary>
/// <param name="QueueDepth">Events waiting to be written.</param>
/// <param name="FullBuildRunning">Whether a full rebuild is in progress.</param>
/// <param name="Ready">Whether a full build has ever completed, i.e. the index is usable.</param>
/// <param name="LastFullBuildCompletedAt">When the last full build succeeded, if ever.</param>
/// <param name="StructuresAvailable">
/// Whether the last build could read the dataset structures container. False means the Structures
/// facet is empty. Null before the first build.
/// </param>
public sealed record IndexSearchStatus(
    int QueueDepth,
    bool FullBuildRunning,
    bool Ready,
    DateTimeOffset? LastFullBuildCompletedAt,
    bool? StructuresAvailable = null);

/// <summary>
/// Client for the IndexSearch service's write API.
/// <para>
/// One method per endpoint, typed to the model each accepts. Deliberately not a generic
/// "send these objects to that route" surface: the compiler should catch a dataset sent to the
/// concepts endpoint, which is exactly the mistake a stringly-typed client makes easy.
/// </para>
/// </summary>
public interface IIndexSearchApiClient
{
    /// <summary>
    /// Sends catalog resources of any kind. One method rather than five: the entry carries its own
    /// <c>Type</c>, so there is no longer a per-kind route to pick — and no way to send a dataset to
    /// the concepts endpoint, which used to index a document nobody would query for.
    /// </summary>
    Task IndexCatalogAsync(IReadOnlyCollection<CatalogIndexEntry> entries, CancellationToken cancellationToken = default);

    Task IndexCodeListEntriesAsync(IReadOnlyCollection<CodeListIndexEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>Removes catalog documents by id. The rows are gone, so there is no payload.</summary>
    Task DeIndexCatalogAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task DeIndexCodeListEntriesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Reads index readiness and queue depth. Cheap enough for a health check.</summary>
    Task<IndexSearchStatus?> GetStatusAsync(CancellationToken cancellationToken = default);
}
