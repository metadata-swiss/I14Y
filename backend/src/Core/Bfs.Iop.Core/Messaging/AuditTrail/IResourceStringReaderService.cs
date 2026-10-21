using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal interface IResourceStringReaderService
{
    Task<string> GetResourceStringAsync(AuditTrailResourceType resourceType, Guid id, CancellationToken cancellationToken = default);
}
