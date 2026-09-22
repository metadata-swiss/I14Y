using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

[TestFixture]
internal sealed class ScheduledReindexTests
{
    private static readonly DateTimeOffset Midnight = new(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);

    [TestCase(6, 2, 1, 0, 2, 0, TestName = "six-hourly, an hour before the first slot")]
    [TestCase(6, 2, 1, 59, 2, 0, TestName = "six-hourly, a minute before the first slot")]
    [TestCase(6, 2, 13, 59, 14, 0, TestName = "six-hourly, mid-day slot")]
    [TestCase(6, 2, 23, 0, 2, 1, TestName = "six-hourly, past the last slot of the day")]
    [TestCase(24, 3, 1, 0, 3, 0, TestName = "nightly, before the slot")]
    [TestCase(24, 3, 2, 59, 3, 0, TestName = "nightly, a minute before the slot")]
    [TestCase(24, 3, 3, 1, 3, 1, TestName = "nightly, just past it, so tomorrow")]
    [TestCase(24, 3, 23, 0, 3, 1, TestName = "nightly, late evening rolls over")]
    public void The_next_slot_is_the_one_after_now(
        int intervalHours,
        int offsetHours,
        int nowHour,
        int nowMinute,
        int dueHour,
        int daysLater)
    {
        var now = Midnight.AddHours(nowHour).AddMinutes(nowMinute);

        NextRun(now, intervalHours, offsetHours)
            .Should().Be(Midnight.AddDays(daysLater).AddHours(dueHour));
    }

    [TestCase(6, 2, 2, TestName = "six-hourly, first slot")]
    [TestCase(6, 2, 8, TestName = "six-hourly, second slot")]
    [TestCase(6, 2, 14, TestName = "six-hourly, third slot")]
    [TestCase(6, 2, 20, TestName = "six-hourly, last slot of the day")]
    [TestCase(24, 3, 3, TestName = "nightly, the only slot")]
    public void A_slot_reached_exactly_moves_on_to_the_next_one(
        int intervalHours,
        int offsetHours,
        int slot)
    {
        var now = Midnight.AddHours(slot);

        NextRun(now, intervalHours, offsetHours).Should().Be(now.AddHours(intervalHours));
    }

    [Test]
    public void The_shipped_default_is_one_pass_a_night()
    {
        var shipped = new ReindexScheduleOptions();

        shipped.Interval.Should().Be(TimeSpan.FromHours(24));
        shipped.Offset.Should().Be(TimeSpan.FromHours(3));
    }

    [Test]
    public async Task Nothing_runs_before_the_first_slot()
    {
        await using var fixture = await Fixture.StartedAtAsync(Midnight.AddHours(1));

        fixture.Time.Advance(TimeSpan.FromMinutes(59));
        await Task.Delay(200);

        fixture.Host.Calls.Should().BeEmpty();
    }

    [Test]
    public async Task The_slot_starts_a_pass()
    {
        await using var fixture = await Fixture.StartedAtAsync(Midnight.AddHours(1));

        fixture.Time.Advance(TimeSpan.FromHours(1));

        (await fixture.PublishedAsync()).Should().BeTrue("the slot should have published a generation");
    }

    [Test]
    public async Task A_slot_that_lands_on_a_running_pass_is_skipped_and_the_next_one_still_fires()
    {
        await using var fixture = await Fixture.StartedAtAsync(Midnight.AddHours(1));

        fixture.Gate.TryBegin("reindex").Should().BeTrue();

        fixture.Time.Advance(TimeSpan.FromHours(1));
        (await fixture.LoggedAsync("because a pass was already running")).Should().BeTrue();

        fixture.Host.Calls.Should().BeEmpty();

        fixture.Gate.End(succeeded: true);

        fixture.Time.Advance(TimeSpan.FromHours(6));

        (await fixture.PublishedAsync()).Should().BeTrue("the schedule should survive a skipped slot");
    }

    [Test]
    public async Task Shutdown_ends_the_loop_without_faulting()
    {
        await using var fixture = await Fixture.StartedAtAsync(Midnight.AddHours(1));

        await fixture.Service.StopAsync(CancellationToken.None);

        fixture.Service.ExecuteTask.Should().NotBeNull();
        fixture.Service.ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
    }

    private static DateTimeOffset NextRun(DateTimeOffset now, int intervalHours, int offsetHours) =>
        ScheduledReindexService.NextRun(
            now,
            TimeSpan.FromHours(intervalHours),
            TimeSpan.FromHours(offsetHours));


    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(
            FakeTimeProvider time,
            ReindexGate gate,
            IndexTestHost host,
            Capture log,
            ReindexScheduleOptions schedule)
        {
            Time = time;
            Gate = gate;
            Host = host;
            Log = log;
            Service = new ScheduledReindexService(
                host.Orchestrator,
                Options.Create(schedule),
                time,
                log);
        }

        public FakeTimeProvider Time { get; }

        public ReindexGate Gate { get; }

        public IndexTestHost Host { get; }

        public Capture Log { get; }

        public ScheduledReindexService Service { get; }


        public static async Task<Fixture> StartedAtAsync(
            DateTimeOffset now,
            ReindexScheduleOptions? schedule = null)
        {
            var time = new FakeTimeProvider(now);
            var gate = new ReindexGate(time);
            var fixture = new Fixture(
                time,
                gate,
                new IndexTestHost(gate),
                new Capture(),
                schedule ?? new ReindexScheduleOptions { IntervalHours = 6, FirstSlotHourUtc = 2 });

            await fixture.Service.StartAsync(CancellationToken.None);

            (await fixture.LoggedAsync("is due at")).Should().BeTrue("the service should be waiting");

            return fixture;
        }

        public Task<bool> PublishedAsync() => EventuallyAsync(() => Host.Calls.Any(x => x == "POST /_aliases"));

        public Task<bool> LoggedAsync(string fragment) =>
            EventuallyAsync(() => Log.Contains(fragment));

        // StopAsync is awaited before anything is torn down. Disposing the service only cancels its
        // token, so without this the loop can still be between the cancelled delay and TryStart when
        // the gate underneath it is disposed — an ObjectDisposedException on a thread nobody watches,
        // surfacing later as a failure in whichever test happened to run next.
        public async ValueTask DisposeAsync()
        {
            await Service.StopAsync(CancellationToken.None);

            Service.Dispose();
            Host.Dispose();
            Gate.Dispose();
        }

        // Bounded, and only ever used to observe work that is already due. Ten seconds rather than
        // three: the happy path returns on the first poll, so the ceiling only matters on a machine
        // busy enough to delay a continuation — a build running alongside the suite, say, which is
        // exactly when a tighter budget failed.
        private static async Task<bool> EventuallyAsync(Func<bool> condition)
        {
            for (var attempt = 0; attempt < 1000; attempt++)
            {
                if (condition())
                {
                    return true;
                }

                await Task.Delay(10);
            }

            return condition();
        }
    }

    private sealed class Capture : ILogger<ScheduledReindexService>
    {
        private readonly List<string> _lines = [];

        public bool Contains(string fragment)
        {
            lock (_lines)
            {
                return _lines.Any(x => x.Contains(fragment, StringComparison.Ordinal));
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            lock (_lines)
            {
                _lines.Add(formatter(state, exception));
            }
        }
    }
}
