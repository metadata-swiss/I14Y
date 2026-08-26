namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record CommitRequest
{
    public required Author Author { get; init; }

    public DateTimeOffset? TimeStamp { get; init; }

    public IEnumerable<ResourceChange> ResourceChanges { get; init; } = [];
}
