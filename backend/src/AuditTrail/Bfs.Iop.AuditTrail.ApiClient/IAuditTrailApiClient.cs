using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.AuditTrail.ApiClient;

public interface IAuditTrailApiClient
{
    Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken);

    Task InitRepositoryAsync(CancellationToken cancellationToken);

    Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken);

    Task CommitAsync(CommitRequest request, CancellationToken cancellationToken);

    Task<PagedResult<Commit>> GetCommitsAsync(
        CommitSearchFilters filters, 
        int? page = null, 
        int? pageSize = null, 
        CancellationToken cancellationToken = default);
}