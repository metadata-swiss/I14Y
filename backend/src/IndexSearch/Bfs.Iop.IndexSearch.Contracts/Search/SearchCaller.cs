using Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record SearchCaller
{
    public static readonly SearchCaller Anonymous = new();

    public BusinessRole Role { get; init; } = BusinessRole.Unknown;
    public IReadOnlyList<string> Agencies { get; init; } = [];
}
