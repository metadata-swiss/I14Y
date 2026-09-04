namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record AgentStatisticsResult
{
    public required AgentModel Publisher { get; init; }

    public IReadOnlyCollection<SearchCountResultItem<SearchResourceType>> Types { get; init; } = [];
}
