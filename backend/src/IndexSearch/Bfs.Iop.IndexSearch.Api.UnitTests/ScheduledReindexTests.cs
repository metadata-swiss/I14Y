using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// The schedule is the only thing that repairs drift: a notification lost to a restart is never
// replayed, and a mapping change stays inert until a pass builds a new generation. A scheduler that
// silently stops running looks exactly like one that works, until the index is wrong.
[TestFixture]
internal sealed class ScheduledReindexTests
{
    private static readonly DateTimeOffset Midnight = new(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);

    // The slots are 02:00, 08:00, 14:00 and 20:00 UTC. These run against the arithmetic directly, so
    // they need no clock and no timers.
    [TestCase(1, 0, 2, 0, 0)]
    [TestCase(1, 59, 2, 0, 0)]
    [TestCase(13, 59, 14, 0, 0)]
    [TestCase(23, 0, 2, 0, 1)]
    public void The_next_slot_is_the_one_after_now(
        int nowHour,
        int nowMinute,
        int dueHour,
        int dueMinute,
        int daysLater)
    {
        var now = Midnight.AddHours(nowHour).AddMinutes(nowMinute);

        ScheduledReindexService.NextRun(now)
            .Should().Be(Midnight.AddDays(daysLater).AddHours(dueHour).AddMinutes(dueMinute));
    }

    [TestCase(2)]
    [TestCase(8)]
    [TestCase(14)]
    [TestCase(20)]
    public void A_slot_reached_exactly_moves_on_to_the_next_one(int slot)
    {
        // The boundary is the whole reason this is a while and not an if. Returning "now" would make
        // the delay zero, and the loop would start a pass for as long as that second lasted.
        var now = Midnight.AddHours(slot);

        ScheduledReindexService.NextRun(now).Should().Be(now.AddHours(6));
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

        // The positive control for the test above: without it, a scheduler that never fired at all
        // would pass that one too.
        (await fixture.PublishedAsync()).Should().BeTrue("the slot should have published a generation");
    }

    [Test]
    public async Task A_slot_that_lands_on_a_running_pass_is_skipped_and_the_next_one_still_fires()
    {
        await using var fixture = await Fixture.StartedAtAsync(Midnight.AddHours(1));

        // Someone triggered a reindex by hand a moment before the slot came round.
        fixture.Gate.TryBegin("reindex").Should().BeTrue();

        fixture.Time.Advance(TimeSpan.FromHours(1));
        (await fixture.LoggedAsync("because a pass was already running")).Should().BeTrue();

        // Refused, not queued: a pass held behind another would start the instant the first ended.
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

        // The cancellation that ends the wait is expected, not an error. A hosted service that
        // faults is reported as a crash on every single shutdown.
        fixture.Service.ExecuteTask.Should().NotBeNull();
        fixture.Service.ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
    }

    // StartAsync returns before ExecuteAsync has run, so advancing the clock straight afterwards
    // races the service: it would wake, read a clock that had already moved past the slot, and
    // schedule the one after it. Every test here waits for the service to announce what it is
    // waiting for before touching the clock.
    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(FakeTimeProvider time, ReindexGate gate, IndexTestHost host, Capture log)
        {
            Time = time;
            Gate = gate;
            Host = host;
            Log = log;
            Service = new ScheduledReindexService(host.Orchestrator, time, log);
        }

        public FakeTimeProvider Time { get; }

        public ReindexGate Gate { get; }

        public IndexTestHost Host { get; }

        public Capture Log { get; }

        public ScheduledReindexService Service { get; }

        public static async Task<Fixture> StartedAtAsync(DateTimeOffset now)
        {
            var time = new FakeTimeProvider(now);
            var gate = new ReindexGate(time);
            var fixture = new Fixture(time, gate, new IndexTestHost(gate), new Capture());

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
