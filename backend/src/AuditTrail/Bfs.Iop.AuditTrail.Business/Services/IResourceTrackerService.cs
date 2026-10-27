using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IResourceTrackerService
{
    Task<RepositoryResponse> InitRepositoryAsync(CancellationToken cancellationToken);

    Task<bool> IsRepositoryInitializedAsync(CancellationToken cancellationToken);

    Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken);

    Task CommitAsync(CommitRequest request, CancellationToken cancellationToken);

    Task<PagedResult<Commit>> GetCommitsAsync(CommitSearchFilters filters, int? page, int? pageSize, CancellationToken cancellationToken);
}
