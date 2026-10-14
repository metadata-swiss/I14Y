using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class ReindexScheduleOptionsValidation : IValidateOptions<ReindexScheduleOptions>
{
    private const int HoursInADay = 24;

    public ValidateOptionsResult Validate(string? name, ReindexScheduleOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.IntervalHours < 1)
        {
            failures.Add(
                $"'{Key(nameof(options.IntervalHours))}' has to be at least 1 hour, not "
                + $"{options.IntervalHours}.");
        }

        if (options.FirstSlotHourUtc is < 0 or > 23)
        {
            failures.Add(
                $"'{Key(nameof(options.FirstSlotHourUtc))}' has to be an hour of the day, 0 to 23, "
                + $"not {options.FirstSlotHourUtc}.");
        }

        if (failures.Count == 0)
        {
            if (options.Interval <= ElasticsearchIndexProvisioner.StaleWindow)
            {
                failures.Add(
                    $"'{Key(nameof(options.IntervalHours))}' is {options.IntervalHours}h, which is "
                    + $"not longer than the orphan sweep window ({ElasticsearchIndexProvisioner.StaleWindow}). "
                    + "Superseded generations would never be old enough to sweep and would accumulate "
                    + "until the disk filled.");
            }

            if (HoursInADay % options.IntervalHours != 0)
            {
                failures.Add(
                    $"'{Key(nameof(options.IntervalHours))}' is {options.IntervalHours}h, which does "
                    + "not divide 24, so the last slot of one day would fall an odd distance from the "
                    + "first of the next. Use 2, 3, 4, 6, 8, 12 or 24.");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }

    private static string Key(string key) => $"{ReindexScheduleOptions.SectionName}:{key}";
}
