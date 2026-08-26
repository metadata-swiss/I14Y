namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record ResourceMetadata
{
    public required string DataFormat { get; init; }

    public required Guid Id { get; init; }

    public required string Identifier { get; init; }

    public required string Type { get; init; }
}
