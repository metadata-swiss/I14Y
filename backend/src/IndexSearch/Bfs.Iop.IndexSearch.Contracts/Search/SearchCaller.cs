namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record SearchCaller
{
    public static readonly SearchCaller Anonymous = new();

    public IndexBusinessRole Role { get; init; } = IndexBusinessRole.Unknown;
    public IReadOnlyList<string> Agencies { get; init; } = [];
}
