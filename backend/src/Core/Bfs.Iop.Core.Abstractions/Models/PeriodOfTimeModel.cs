namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record PeriodOfTimeModel
{
    public DateTimeOffset? Start { get; init; }

    public DateTimeOffset? End { get; init; }
}
