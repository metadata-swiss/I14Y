namespace Bfs.Iop.IndexSearch.Api.Hosting;

/// <summary>
///     When <see cref="ScheduledReindexService" /> rebuilds the whole index. Hours rather than
///     TimeSpans: a malformed TimeSpan string fails inside the options binder, before
///     <see cref="ReindexScheduleOptionsValidation" /> runs, so the operator would get a framework
///     message instead of one naming the key and saying what is wrong with it.
/// </summary>
internal sealed class ReindexScheduleOptions
{
    public const string SectionName = "ReindexSchedule";

    public int IntervalHours { get; set; } = 24;

    public int FirstSlotHourUtc { get; set; } = 3;

    public TimeSpan Interval => TimeSpan.FromHours(IntervalHours);

    public TimeSpan Offset => TimeSpan.FromHours(FirstSlotHourUtc);
}
