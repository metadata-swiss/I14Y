namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record PagedResult<T> where T : class
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public IReadOnlyCollection<T> Results { get; init; } = [];

    public int TotalCount { get; init; }
}
