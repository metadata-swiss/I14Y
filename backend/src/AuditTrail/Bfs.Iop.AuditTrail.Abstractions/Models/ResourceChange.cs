namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record ResourceChange
{
    public string? Data { get; init; }

    public required ResourceChangeOperation Operation { get; init; }

    public required ResourceMetadata ResourceMetadata { get; init; }
}