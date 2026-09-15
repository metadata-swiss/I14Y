using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

/// <summary>
///     Writing the index. Everything here is derived from Postgres, so nothing in these indices is
///     lost by rebuilding them — only the time it takes.
/// </summary>
[ApiController]
[Route("api/index")]
[Produces("application/json")]
// The same Keycloak/eIAM token the search endpoints read for authorization, rather than a shared
// secret: a rebuild is then attributable to whoever asked for it, and there is one way in, not two.
[Authorize(Policy = IndexPolicies.Rebuild)]
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
            ReindexCounts.From(status.CatalogReport),
            ReindexCounts.From(status.CodeListReport));
    }
}
