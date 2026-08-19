using Bfs.Iop.Core.Abstractions.Models;
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
    private readonly ILogger<IndexController> _logger;

    public IndexController(IIndexEventQueue queue, IIndexBuildState buildState, ILogger<IndexController> logger)
    {
        _queue = queue;
        _buildState = buildState;
        _logger = logger;
    }

    /// <summary>Current state of the indexes and of the ingest queue.</summary>
    public sealed record IndexStatusResponse(
        int QueueDepth,
        bool FullBuildRunning,
        bool Ready,
        DateTimeOffset? LastFullBuildCompletedAt);

    /// <summary>Queues datasets for indexing.</summary>
    [HttpPost("datasets")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexDatasets([FromBody] DcatDatasetModel[] models) =>
        Enqueue(IndexTarget.Catalog, models, x => x.Id);

    /// <summary>Queues data services for indexing.</summary>
    [HttpPost("data-services")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexDataServices([FromBody] DataServiceModel[] models) =>
        Enqueue(IndexTarget.Catalog, models, x => x.Id);

    /// <summary>Queues public services for indexing.</summary>
    [HttpPost("public-services")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexPublicServices([FromBody] PublicServiceModel[] models) =>
        Enqueue(IndexTarget.Catalog, models, x => x.Id);

    /// <summary>Queues concepts for indexing.</summary>
    [HttpPost("concepts")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexConcepts([FromBody] IopConceptModel[] models) =>
        Enqueue(IndexTarget.Catalog, models, x => x.Id);

    /// <summary>Queues mapping tables for indexing.</summary>
    [HttpPost("mapping-tables")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexMappingTables([FromBody] MappingTableModel[] models) =>
        Enqueue(IndexTarget.Catalog, models, x => x.Id);

    /// <summary>Queues code-list entries for indexing.</summary>
    [HttpPost("codelist-entries")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult IndexCodeListEntries([FromBody] CodeListEntryModel[] models) =>
        Enqueue(IndexTarget.CodeListEntry, models, x => x.Id);

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

    /// <summary>Reports index readiness, queue depth and the last successful full build.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<IndexStatusResponse> GetStatus() =>
        Ok(new IndexStatusResponse(
            _queue.Count,
            _buildState.IsFullBuildRunning,
            _buildState.IsReady,
            _buildState.LastFullBuildCompletedAt));

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
