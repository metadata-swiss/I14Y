namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record Author
{
    public required string Email { get; init; }

    public required string Name { get; init; }
}
