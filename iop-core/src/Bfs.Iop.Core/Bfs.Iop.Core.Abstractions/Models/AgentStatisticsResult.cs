using Bfs.Iop.Core.Abstractions.Models.Search;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record AgentStatisticsResult
{
    public required AgentModel Publisher { get; init; }

    public IEnumerable<SearchCountResultItem<SearchResourceType>> Types { get; init; } = [];
}
