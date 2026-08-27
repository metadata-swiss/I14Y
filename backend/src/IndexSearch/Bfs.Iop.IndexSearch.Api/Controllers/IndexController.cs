using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.IndexSearch.Api.Filters;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
/// Write side of the index: re-index triggers from IOP Core, index status and forced rebuilds.
/// <para>
/// Each trigger carries the resource itself, so this service never has to read it back. Every
/// endpoint answers 202 immediately — the document is written by the background worker.
/// </para>
/// <para>
/// Protected by a shared secret rather than a user token: the caller is a service, not a person.
/// </para>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[IndexApiSecret]
public sealed class IndexController : ControllerBase
{
    private readonly IIndexEventQueue _queue;
    private readonly IIndexBuildState _buildState;
    private readonly IIndexRebuildTrigger _rebuildTrigger;
    private readonly ILogger<IndexController> _logger;

    public IndexController(
        IIndexEventQueue queue,
        IIndexBuildState buildState,
        IIndexRebuildTrigger rebuildTrigger,
        ILogger<IndexController> logger)
    {
        _queue = queue;
        _buildState = buildState;
        _rebuildTrigger = rebuildTrigger;
        _logger = logger;
    }

    /// <summary>Current state of the indexes and of the ingest queue.</summary>
    /// <param name="QueueDepth">Events waiting to be written.</param>
    /// <param name="FullBuildRunning">Whether a full rebuild is in progress.</param>
    /// <param name="Ready">Whether a full build has completed, i.e. the index is usable.</param>
    /// <param name="LastFullBuildCompletedAt">When the last full build succeeded, if ever.</param>
    /// <param name="StructuresAvailable">
    /// Whether the last build could read the dataset structures container. False means the Structures
    /// facet is empty because every dataset was indexed as structure-less — reported here so that a
    /// missing facet is diagnosable instead of looking like a broken filter. Null before the first build.
    /// </param>
    public sealed record IndexStatusResponse(
        int QueueDepth,
        bool FullBuildRunning,
        bool Ready,
        DateTimeOffset? LastFullBuildCompletedAt,
        bool? StructuresAvailable);

    /// <summary>
    /// Queues catalog resources for indexing.
    /// <para>
    /// One route for all five kinds. The entry carries its own <c>Type</c>, so there is no per-kind
    /// endpoint to choose — and therefore no way to POST a dataset to the concepts route by mistake,
    /// which previously produced a document nobody would ever query for, with no error anywhere.
    /// </para>
    /// </summary>
    [HttpPost("catalog")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexCatalog([FromBody] CatalogIndexEntry[] entries) =>
        Enqueue(IndexTarget.Catalog, entries, x => x.Id);

    /// <summary>Queues code-list entries for indexing.</summary>
    [HttpPost("codelist-entries")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexCodeListEntries([FromBody] CodeListIndexEntry[] entries) =>
        Enqueue(IndexTarget.CodeListEntry, entries, x => x.Id);

    /// <summary>Queues catalog documents for removal. No payload needed — the rows are gone.</summary>
    [HttpDelete("catalog")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult DeIndexCatalog([FromBody] Guid[] ids) => EnqueueRemovals(IndexTarget.Catalog, ids);

    /// <summary>Queues code-list-entry documents for removal.</summary>
    [HttpDelete("codelist-entries")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult DeIndexCodeListEntries([FromBody] Guid[] ids) => EnqueueRemovals(IndexTarget.CodeListEntry, ids);

    /// <summary>
    /// Rebuilds both indexes from the database without waiting for the next scheduled run, writing
    /// over the existing documents.
    /// <para>
    /// <b>This does not purge.</b> The rebuild indexes every row it reads and deletes nothing, so a
    /// document whose row no longer exists in the database survives it, and a mapping change does not
    /// take effect. Use <see cref="Recreate"/> for either of those.
    /// </para>
    /// <para>
    /// Returns 202 and rebuilds in the background. A rebuild already in progress is reported rather
    /// than queued a second time, because two concurrent rebuilds would fight over the same documents.
    /// </para>
    /// </summary>
    [HttpPost("rebuild")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Rebuild() => RequestBuild(recreate: false);

    /// <summary>
    /// Drops both indexes and builds them again from the database — the same thing a restart of this
    /// service does.
    /// <para>
    /// This is the only on-demand operation that <b>purges</b>: documents whose rows no longer exist
    /// are gone afterwards, and a changed mapping takes effect. It is what repairs a de-index event
    /// that was lost, which the in-memory forward queue makes an ordinary occurrence.
    /// </para>
    /// <para>
    /// <b>Destructive, and the cost is user-visible.</b> From the moment the indexes are dropped until
    /// the build completes, every search returns HTTP 200 with no results — which reads as missing
    /// data, not as work in progress. If the build then fails (most often an unreachable database)
    /// the indexes stay empty until this is called again. Prefer <see cref="Rebuild"/> unless a purge
    /// or a mapping change is actually needed.
    /// </para>
    /// </summary>
    [HttpPost("recreate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Recreate() => RequestBuild(recreate: true);

    /// <summary>
    /// 409 is a courtesy, not a lock: a build can start between this check and the request landing,
    /// in which case the request is accepted and runs after the one in progress. That costs one extra
    /// rebuild and never a lost or interleaved one — the builder serialises them on its own gate —
    /// so it is not worth taking a lock across an HTTP handler to close.
    /// </summary>
    private IActionResult RequestBuild(bool recreate)
    {
        if (_buildState.IsFullBuildRunning)
        {
            return Conflict("A full index build is already running.");
        }

        _rebuildTrigger.Request(recreate);

        return Accepted();
    }

    /// <summary>Reports index readiness, queue depth and the last successful full build.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<IndexStatusResponse> GetStatus() =>
        Ok(new IndexStatusResponse(
            _queue.Count,
            _buildState.IsFullBuildRunning,
            _buildState.IsReady,
            _buildState.LastFullBuildCompletedAt,
            _buildState.StructuresAvailable));

    private IActionResult Enqueue<T>(IndexTarget target, T[]? models, Func<T, Guid> idOf)
    {
        if (models is null || models.Length == 0)
        {
            return BadRequest("At least one resource is required.");
        }

        return EnqueueAll(models.Select(x => new IndexEvent(target, idOf(x), x)), target);
    }

    private IActionResult EnqueueRemovals(IndexTarget target, Guid[]? ids)
    {
        if (ids is null || ids.Length == 0)
        {
            return BadRequest("At least one id is required.");
        }

        return EnqueueAll(ids.Select(id => new IndexEvent(target, id, Payload: null)), target);
    }

    private IActionResult EnqueueAll(IEnumerable<IndexEvent> events, IndexTarget target)
    {
        foreach (var indexEvent in events)
        {
            if (_queue.TryEnqueue(indexEvent))
            {
                continue;
            }

            // Answer with backpressure rather than dropping silently, so the caller's circuit
            // breaker reacts and the drift is visible instead of invisible.
            _logger.LogWarning("Index queue is full; rejected a {Target} trigger.", target);
            return StatusCode(StatusCodes.Status429TooManyRequests, "The index queue is full.");
        }

        return Accepted();
    }
}
