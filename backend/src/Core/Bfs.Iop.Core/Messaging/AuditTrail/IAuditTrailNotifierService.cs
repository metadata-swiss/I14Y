using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal interface IAuditTrailNotifierService
{
    Task EnsureResourceIsTrackedAsync(
        AuditTrailResourceType resourceType,
        Guid id, 
        CancellationToken cancellationToken = default);

    Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);

    Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);

    Task NotifyResourceDeletedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);
}
