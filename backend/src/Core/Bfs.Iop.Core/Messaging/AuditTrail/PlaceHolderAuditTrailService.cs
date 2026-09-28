using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal class PlaceHolderAuditTrailService : IAuditTrailNotifierService
{
    public Task EnsureResourceIsTrackedAsync(AuditTrailResourceType resourceType, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyResourceCreatedAsync(AuditTrailResourceType resourceType, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyResourceDeletedAsync(AuditTrailResourceType resourceType, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyResourceUpdatedAsync(AuditTrailResourceType resourceType, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
