namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record PeriodOfTimeModel
{
    public DateTimeOffset? Start { get; init; }

    public DateTimeOffset? End { get; init; }
}
