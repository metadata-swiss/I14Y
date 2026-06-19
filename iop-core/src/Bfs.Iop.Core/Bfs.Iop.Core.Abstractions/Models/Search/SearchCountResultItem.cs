namespace Bfs.Iop.Core.Abstractions.Models.Search;

public sealed record SearchCountResultItem<T>
{
    public int Count { get; init; }

    public required T Value { get; init; }
}
