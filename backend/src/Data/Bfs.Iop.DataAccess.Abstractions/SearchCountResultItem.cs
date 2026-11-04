namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record SearchCountResultItem<T>
{
    public int Count { get; init; }

    public required T Value { get; init; }

    // The value as the search index names it, e.g. "vocabulary!code" for a theme.
    public string? SearchKey { get; init; }
}
