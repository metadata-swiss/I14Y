using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal interface IAuditTrailNotifierService
{
    Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType,
        IReadOnlyModel resource,
        CancellationToken cancellationToken = default);

    Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType,
        IReadOnlyModel resource,
        CancellationToken cancellationToken = default);

    Task NotifyResourceDeletedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default);

    Task NotifyFileCreated(CancellationToken cancellationToken = default);

    Task NotifyFileUpdated(CancellationToken cancellationToken = default);

    Task NotifyFileDeleted(CancellationToken cancellationToken = default);
}
