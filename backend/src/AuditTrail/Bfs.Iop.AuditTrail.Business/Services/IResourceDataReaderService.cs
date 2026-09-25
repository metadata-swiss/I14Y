using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Helpers;

namespace Bfs.Iop.AuditTrail.Business.Services;

public interface IResourceDataReaderService
{
    Task<Stream> GetResourceDataAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);
}
