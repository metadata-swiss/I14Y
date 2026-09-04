using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Configuration;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IGitWrapper
{
    GitOptions GitOptions { get; }

    Task<RepositoryResponse> ExecuteAsync(string[] arguments, CancellationToken cancellationToken);

    Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken);

    Task<RepositoryResponse> InitializeRepositoryAsync(CancellationToken cancellationToken);
}
