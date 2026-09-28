using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.AuditTrail.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuditTrailController : ControllerBase
{
    private readonly IResourceTrackerService _fileTrackerService;

    public AuditTrailController(IResourceTrackerService fileTrackerService) =>
        _fileTrackerService = fileTrackerService;

    [HttpPost("repository-init")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitRepository(CancellationToken cancellationToken)
    {
        var response = await _fileTrackerService.InitRepositoryAsync(cancellationToken);

        if (response.Success)
        {
            return CreatedAtAction(nameof(InitRepository), response);
        }

        if (response.ExitCode == 409)
        {
            return Conflict(response.StdErr);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, response.StdErr);
    }

    [HttpGet]
    [Route("repository-exists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<bool> IsRepositoryInitialized(CancellationToken cancellationToken) =>
        _fileTrackerService.IsRepositoryInitializedAsync(cancellationToken);

    [HttpGet]
    [Route("resource-tracked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<bool> IsResourceTrackedAsync([FromQuery] ResourceMetadata metadata, CancellationToken cancellationToken) =>
        _fileTrackerService.IsResourceTrackedAsync(metadata, cancellationToken);

    [HttpPost]
    [Route("commit")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        await _fileTrackerService.CommitAsync(request, cancellationToken);

        return Accepted();
    }

    [HttpGet]
    [Route("commits")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Commit>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IEnumerable<Commit>> GetCommitsAsync(
        [FromQuery] CommitSearchFilters filters,
        CancellationToken cancellationToken) => _fileTrackerService.GetCommitsAsync(filters, cancellationToken);
}
