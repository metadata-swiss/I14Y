using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IResourceDataReaderService
{
    Task<Stream> GetResourceDataAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);
}
