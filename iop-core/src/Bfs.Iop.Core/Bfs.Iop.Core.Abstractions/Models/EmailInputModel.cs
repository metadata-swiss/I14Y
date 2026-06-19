namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record EmailInputModel
{
    public required string Email { get; init; }
}
