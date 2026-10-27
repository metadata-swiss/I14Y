using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IGitCommitProcessorService
{
    Task<RepositoryResponse> ProcessCommitAsync(CommitRequest request, CancellationToken cancellationToken);
}
