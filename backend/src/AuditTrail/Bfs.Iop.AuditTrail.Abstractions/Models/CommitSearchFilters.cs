namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record CommitSearchFilters
{
    // Author email or author name.
    public string? Author { get; init; }

    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public Guid? ResourceId { get; init; }

    public string? ResourceIdentifier { get; init; }

    public string? ResourceType { get; init; }
}