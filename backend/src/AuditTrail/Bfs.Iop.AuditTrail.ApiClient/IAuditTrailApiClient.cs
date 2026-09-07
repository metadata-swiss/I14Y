using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.ApiClient;

public interface IAuditTrailApiClient
{
    Task<bool> GetRepositoryExistsAsync(CancellationToken cancellationToken);

    Task InitRepositoryAsync(CancellationToken cancellationToken);

    Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken);

    Task CommitAsync(CommitRequest request, CancellationToken cancellationToken);
}