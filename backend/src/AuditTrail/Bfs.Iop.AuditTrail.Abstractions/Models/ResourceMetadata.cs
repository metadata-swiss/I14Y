namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record ResourceMetadata
{
    public required string Filename { get; init; }

    public required Guid Id { get; init; }

    public required string ResourceType { get; init; }
}
