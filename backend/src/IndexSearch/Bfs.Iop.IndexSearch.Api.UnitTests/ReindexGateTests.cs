using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Hosting;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// A reindex reads the whole database and writes half a million documents. Two at once do that
// twice, and the startup pass overlapping a called one is the easy way to get there.
[TestFixture]
internal sealed class ReindexGateTests
{
    [Test]
    public async Task A_reindex_runs_when_nothing_else_is_running()
    {
        using var gate = new ReindexGate();

        var ran = false;

        (await gate.TryRunAsync("reindex", () => { ran = true; return Task.CompletedTask; })).Should().BeTrue();
        ran.Should().BeTrue();
    }

    [Test]
    public async Task A_second_reindex_is_refused_while_the_first_is_still_running()
    {
        using var gate = new ReindexGate();

        var first = new TaskCompletionSource();
        var secondRan = false;

        var running = gate.TryRunAsync("reindex", () => first.Task);

        // Refused immediately rather than queued: waiting minutes for a second identical reindex helps
        // nobody, and the caller needs to know one is already in flight.
        var second = await gate.TryRunAsync("reindex", () => { secondRan = true; return Task.CompletedTask; });

        second.Should().BeFalse();
        secondRan.Should().BeFalse();

        first.SetResult();
        (await running).Should().BeTrue();
    }

    [Test]
    public async Task The_gate_reopens_once_a_reindex_finishes()
    {
        using var gate = new ReindexGate();

        await gate.TryRunAsync("reindex", () => Task.CompletedTask);

        (await gate.TryRunAsync("reindex", () => Task.CompletedTask)).Should().BeTrue();
    }

    [Test]
    public async Task A_reindex_that_throws_still_reopens_the_gate()
    {
        using var gate = new ReindexGate();

        var failing = async () => await gate.TryRunAsync(
            "reindex",
            () => throw new InvalidOperationException("the index refused everything"));

        await failing.Should().ThrowAsync<InvalidOperationException>();

        // Otherwise one failed reindex would block every later one until the host restarted.
        (await gate.TryRunAsync("reindex", () => Task.CompletedTask)).Should().BeTrue();
    }

    [Test]
    public async Task The_gate_reports_whether_a_reindex_is_in_flight()
    {
        using var gate = new ReindexGate();

        gate.IsRunning.Should().BeFalse();

        var started = new TaskCompletionSource();
        var finish = new TaskCompletionSource();

        var running = gate.TryRunAsync("reindex", async () =>
        {
            started.SetResult();
            await finish.Task;
        });

        await started.Task;
        gate.IsRunning.Should().BeTrue();

        finish.SetResult();
        await running;

        gate.IsRunning.Should().BeFalse();
    }
}
