namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record SearchCountResultItem<T>
{
    public int Count { get; init; }

    public required T Value { get; init; }
}
