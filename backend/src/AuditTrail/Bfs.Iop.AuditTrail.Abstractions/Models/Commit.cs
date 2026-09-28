namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record Commit
{
    public required Author Author { get; init; }

    public IEnumerable<string> Changes { get; init; } = [];

    public required string Sha { get; init; }

    public DateTimeOffset TimeStamp { get; init; }
}
