using Bfs.Iop.IndexSearch.Api.Filters;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
///     Writing the index. Everything here is derived from Postgres, so nothing in these indices is
///     lost by rebuilding them — only the time it takes.
/// </summary>
[ApiController]
[Route("api/index")]
[Produces("application/json")]
[ServiceFilter(typeof(IndexApiKeyFilter))]
public sealed class IndexController : ControllerBase
{
    private readonly ReindexOrchestrator _orchestrator;
    private readonly ReindexGate _gate;

    public IndexController(ReindexOrchestrator orchestrator, ReindexGate gate)
    {
        _orchestrator = orchestrator;
        _gate = gate;
    }

    /// <summary>
    ///     Starts a full pass and returns immediately. A pass takes minutes, which is longer than a
    ///     proxy will hold a connection open, so it deliberately does not run on the request:
    ///     the response says it started and <c>GET /api/index/status</c> says how it is going.
    /// </summary>
    /// <param name="reset">
    ///     Rebuild into fresh indices and swap the aliases at the end, which is what applies a
    ///     changed mapping. Without it the pass writes over the live documents in place.
    /// </param>
    [HttpPost("reindex")]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Reindex([FromQuery] bool reset = false)
    {
        if (!_orchestrator.TryStart(reset))
        {
            return Conflict(new { message = $"A {_gate.Operation} is already running." });
        }

        return AcceptedAtAction(nameof(Status), Snapshot());
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IndexStatusResponse), StatusCodes.Status200OK)]
    public ActionResult<IndexStatusResponse> Status() => Ok(Snapshot());

    private IndexStatusResponse Snapshot() => new(
        _gate.IsRunning,
        _gate.Operation,
        _gate.StartedAt,
        _gate.FinishedAt,
        _gate.Elapsed,
        _gate.LastSucceeded,
        ReindexCounts.From(_gate.LastCatalogReport),
        ReindexCounts.From(_gate.LastCodeListReport));
}
