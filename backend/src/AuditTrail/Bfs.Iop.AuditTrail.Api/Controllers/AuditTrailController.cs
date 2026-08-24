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

    [HttpGet]
    [Route("repository-init")]
    public Task<RepositoryResponse> InitRepository(CancellationToken cancellationToken) =>
        _fileTrackerService.InitRepositoryAsync(cancellationToken);

    [HttpGet]
    [Route("repository-exists")]
    public Task<bool> IsRepositoryInitialized(CancellationToken cancellationToken) =>
        _fileTrackerService.IsRepositoryInitializedAsync(cancellationToken);

    [HttpGet]
    [Route("resource-tracked")]
    public Task<bool> IsResourceTrackedAsync([FromQuery] ResourceMetadata metadata, CancellationToken cancellationToken) =>
        _fileTrackerService.IsResourceTrackedAsync(metadata, cancellationToken);

    [HttpPost]
    [Route("commit")]
    public Task CommitAsync(CommitRequest request, CancellationToken cancellationToken) =>
        _fileTrackerService.CommitAsync(request, cancellationToken);

    [HttpGet]
    [Route("commits")]
    public Task<IEnumerable<Commit>> GetCommitsAsync(
        [FromQuery] CommitSearchFilters filters,
        CancellationToken cancellationToken) => _fileTrackerService.GetCommitsAsync(filters, cancellationToken);
}
