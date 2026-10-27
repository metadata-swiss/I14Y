namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record CommitRequest
{
    public required Author Author { get; init; }

    public string? CustomMessage { get; init; }

    public required IEnumerable<ResourceChange> ResourceChanges { get; init; }
}
