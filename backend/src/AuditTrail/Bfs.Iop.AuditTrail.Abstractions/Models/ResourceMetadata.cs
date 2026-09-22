namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record ResourceMetadata
{
    public required Guid Id { get; init; }

    public required AuditTrailResourceType ResourceType { get; init; }
}
