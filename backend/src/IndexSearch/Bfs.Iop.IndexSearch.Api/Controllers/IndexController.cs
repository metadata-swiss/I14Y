using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

[ApiController]
[Route("api/index")]
[Produces("application/json")]
[Authorize(Policy = IndexPolicies.Rebuild)]
public sealed class IndexController : ControllerBase
{
    private readonly ReindexOrchestrator _orchestrator;
    private readonly ReindexGate _gate;
    private readonly PendingIndexWrites _pending;
    private readonly IIncrementalIndexWriter _writer;

    public IndexController(
        ReindexOrchestrator orchestrator,
        ReindexGate gate,
        PendingIndexWrites pending,
        IIncrementalIndexWriter writer)
    {
        _orchestrator = orchestrator;
        _gate = gate;
        _pending = pending;
        _writer = writer;
    }

    /// <summary>
    ///     Starts a full pass and returns immediately. A pass takes minutes, which is longer than a
    ///     proxy will hold a connection open, so it deliberately does not run on the request:
    ///     the response says it started and <c>GET /api/index/status</c> says how it is going.
    /// </summary>
    [HttpPost("reindex")]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Reindex()
    {
        if (!_orchestrator.TryStart())
        {
            return Conflict(new { message = $"A {_gate.Operation} is already running." });
        }

        return AcceptedAtAction(nameof(Status), Snapshot());
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<IndexStatusResponse> Status() => Ok(Snapshot());

    /// <summary>
    ///     Brings one catalogue resource up to date. The resource is re-read here rather than sent by
    ///     the caller, so the index can never be given a version of it that was already stale when the
    ///     call was made.
    /// </summary>
    [AllowAnonymous]
    [HttpPut("catalog/{type}/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertCatalogResource(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken)
    {
        Remember(new PendingIndexWrite(PendingIndexTarget.CatalogResource, id, PendingIndexOperation.Upsert, type));

        await _writer.UpsertCatalogResourceAsync(type, id, cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpDelete("catalog/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveCatalogResource(Guid id, CancellationToken cancellationToken)
    {
        Remember(new PendingIndexWrite(PendingIndexTarget.CatalogResource, id, PendingIndexOperation.Remove));

        await _writer.RemoveCatalogResourceAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Replaces every code list entry of one concept. The whole concept is the unit because an
    ///     entry's ancestor codes are derived from the entries around it.
    /// </summary>
    [AllowAnonymous]
    [HttpPut("codelist/{conceptId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReplaceCodeList(Guid conceptId, CancellationToken cancellationToken)
    {
        Remember(new PendingIndexWrite(PendingIndexTarget.CodeList, conceptId, PendingIndexOperation.Upsert));

        await _writer.ReplaceCodeListAsync(conceptId, cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpDelete("codelist/{conceptId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveCodeList(Guid conceptId, CancellationToken cancellationToken)
    {
        Remember(new PendingIndexWrite(PendingIndexTarget.CodeList, conceptId, PendingIndexOperation.Remove));

        await _writer.RemoveCodeListAsync(conceptId, cancellationToken);

        return NoContent();
    }

    /// <summary>
    ///     Records a write for replay if a rebuild is still able to lose it.
    /// </summary>
    private void Remember(PendingIndexWrite write) => _pending.Record(write);

    private IndexStatusResponse Snapshot()
    {
        // One read: the pass runs on another thread, and reading field by field could describe a state
        // the gate was never actually in.
        var status = _gate.Status;

        return new IndexStatusResponse(
            status.IsRunning,
            status.Operation,
            status.StartedAt,
            status.FinishedAt,
            _gate.Elapsed,
            status.LastSucceeded,
            Counts(status.CatalogReport),
            Counts(status.CodeListReport));
    }

    private static ReindexCounts? Counts(IndexRebuildReport? report) => report is null
        ? null
        : new ReindexCounts(
            report.DocumentsSent,
            report.DocumentsWritten,
            report.BatchesFailed,
            report.StructuresResolved);
}
