namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record ResourceChange
{
    public required ResourceChangeOperation Operation { get; init; }

    public required ResourceMetadata ResourceMetadata { get; init; }

    public required string Data { get; init; }
}
