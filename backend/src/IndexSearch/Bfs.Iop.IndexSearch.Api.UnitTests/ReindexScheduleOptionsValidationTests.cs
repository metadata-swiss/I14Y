using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

[TestFixture]
internal sealed class ReindexScheduleOptionsValidationTests
{
    [Test]
    public void The_shipped_default_is_accepted()
    {
        Validate(new ReindexScheduleOptions()).Failed.Should().BeFalse();
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(6)]
    [TestCase(8)]
    [TestCase(12)]
    [TestCase(24)]
    public void An_interval_that_divides_the_day_and_clears_the_sweep_is_accepted(int hours)
    {
        Validate(Options(intervalHours: hours)).Failed.Should().BeFalse();
    }

    [TestCase(0, TestName = "an interval of zero")]
    [TestCase(-6, TestName = "a negative interval")]
    public void An_interval_that_would_never_advance_is_rejected(int hours)
    {
        var result = Validate(Options(intervalHours: hours));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ReindexSchedule:IntervalHours");
    }

    [Test]
    public void An_interval_inside_the_sweep_window_is_rejected()
    {
        var result = Validate(Options(intervalHours: 1));

        result.Failed.Should().BeTrue();

        result.FailureMessage.Should().Contain("sweep");
        result.FailureMessage.Should().Contain("accumulate");
    }

    [Test]
    public void The_sweep_guard_tracks_the_real_window_rather_than_a_copy_of_it()
    {
        var atTheWindow = (int)ElasticsearchIndexProvisioner.StaleWindow.TotalHours;

        Validate(Options(intervalHours: atTheWindow)).Failed.Should().BeTrue();
    }

    [TestCase(5, TestName = "five hours")]
    [TestCase(7, TestName = "seven hours")]
    [TestCase(9, TestName = "nine hours")]
    public void An_interval_that_does_not_divide_the_day_is_rejected(int hours)
    {
        var result = Validate(Options(intervalHours: hours));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("divide 24");
    }

    [TestCase(-1, TestName = "before the day starts")]
    [TestCase(24, TestName = "one past the last hour")]
    public void A_first_slot_outside_the_day_is_rejected(int hour)
    {
        var result = Validate(Options(firstSlotHourUtc: hour));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ReindexSchedule:FirstSlotHourUtc");
    }

    [Test]
    public void Every_problem_is_reported_at_once()
    {
        var result = Validate(Options(intervalHours: 0, firstSlotHourUtc: 99));

        result.Failures.Should().HaveCount(2);
    }

    private static Microsoft.Extensions.Options.ValidateOptionsResult Validate(ReindexScheduleOptions options) =>
        new ReindexScheduleOptionsValidation().Validate(name: null, options);

    private static ReindexScheduleOptions Options(int intervalHours = 24, int firstSlotHourUtc = 3) =>
        new() { IntervalHours = intervalHours, FirstSlotHourUtc = firstSlotHourUtc };
}
