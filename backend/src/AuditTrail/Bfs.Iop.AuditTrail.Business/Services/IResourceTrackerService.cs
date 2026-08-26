using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IResourceTrackerService
{
    Task<RepositoryResponse> InitRepositoryAsync(CancellationToken cancellationToken);

    Task<bool> IsRepositoryInitializedAsync(CancellationToken cancellationToken);

    Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken);

    Task CommitAsync(CommitRequest request, CancellationToken cancellationToken);

    Task<IEnumerable<Commit>> GetCommitsAsync(CommitSearchFilters filters,  CancellationToken cancellationToken);
}
