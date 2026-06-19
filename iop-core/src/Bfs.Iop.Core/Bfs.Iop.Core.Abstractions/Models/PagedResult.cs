namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record PagedResult<T> where T : class
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public IEnumerable<T> Results { get; init; } = [];

    public int TotalCount { get; init; }
}
