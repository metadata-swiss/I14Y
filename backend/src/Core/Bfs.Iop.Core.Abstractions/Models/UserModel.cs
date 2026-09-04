using Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record UserModel
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? Email { get; init; }

    public BusinessRole BusinessRole { get; init; }

    public IEnumerable<IdentifierNameModel> Agents { get; init; } = [];
}
