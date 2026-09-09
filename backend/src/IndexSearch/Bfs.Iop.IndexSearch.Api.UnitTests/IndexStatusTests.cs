using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Bfs.Iop.IndexSearch.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// The index filling up does not mean a reindex has finished — documents are written in batches as
// they are read, so a count in Elasticsearch climbs throughout the run. Status is the only way to
// ask the host itself, which makes "running" being right the whole point of it.
[TestFixture]
internal sealed class IndexStatusTests
{
    private static readonly IndexRebuildReport CatalogReport = new(2935, 2935, 0, StructuresResolved: true);
    private static readonly IndexRebuildReport CodeListReport = new(490754, 490754, 0);

    [Test]
    public void Before_anything_has_run_status_reports_an_idle_host()
    {
        using var gate = new ReindexGate();

        var status = Status(gate);

        status.Running.Should().BeFalse();
        status.Operation.Should().BeNull();
        status.StartedAt.Should().BeNull();
        status.Elapsed.Should().BeNull();
        status.LastSucceeded.Should().BeNull();
        status.Catalog.Should().BeNull();
    }

    [Test]
    public async Task While_a_reindex_runs_status_names_it_and_counts_up()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 3, 8, 0, 0, TimeSpan.Zero));

        using var gate = new ReindexGate(time);

        var finish = new TaskCompletionSource();
        var running = gate.TryRunAsync("reindex", () => finish.Task);

        time.Advance(TimeSpan.FromMinutes(2));

        var status = Status(gate);

        status.Running.Should().BeTrue();
        status.Operation.Should().Be("reindex");
        status.StartedAt.Should().Be(new DateTimeOffset(2026, 9, 3, 8, 0, 0, TimeSpan.Zero));

        // Null while it runs, so a caller cannot read a finish time for something still going.
        status.FinishedAt.Should().BeNull();
        status.LastSucceeded.Should().BeNull();

        status.Elapsed.Should().Be(TimeSpan.FromMinutes(2));

        finish.SetResult();
        await running;
    }

    [Test]
    public async Task A_reset_pass_is_named_apart_from_a_plain_one()
    {
        using var gate = new ReindexGate();

        var finish = new TaskCompletionSource();
        var running = gate.TryRunAsync("reindex (reset)", () => finish.Task);

        Status(gate).Operation.Should().Be("reindex (reset)");

        finish.SetResult();
        await running;
    }

    [Test]
    public async Task After_a_reindex_status_reports_how_it_went()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 9, 3, 8, 0, 0, TimeSpan.Zero));

        using var gate = new ReindexGate(time);

        gate.TryBegin("reindex").Should().BeTrue();

        time.Advance(TimeSpan.FromSeconds(160));

        gate.End(succeeded: true, CatalogReport, CodeListReport);

        var status = Status(gate);

        status.Running.Should().BeFalse();
        status.LastSucceeded.Should().BeTrue();
        status.Elapsed.Should().Be(TimeSpan.FromSeconds(160));

        // "Did it work" is the question once a reindex is over, and sent-versus-written is what
        // answers it.
        status.Catalog.Should().Be(ReindexCounts.From(CatalogReport));
        status.CodeLists.Should().Be(ReindexCounts.From(CodeListReport));
    }

    [Test]
    public async Task A_reindex_that_threw_is_reported_as_not_succeeded()
    {
        using var gate = new ReindexGate();

        var failing = async () => await gate.TryRunAsync(
            "rebuild",
            () => throw new InvalidOperationException("the index refused everything"));

        await failing.Should().ThrowAsync<InvalidOperationException>();

        var status = Status(gate);

        status.Running.Should().BeFalse();

        // Otherwise a failed rebuild is indistinguishable from one that never ran.
        status.LastSucceeded.Should().BeFalse();
        status.FinishedAt.Should().NotBeNull();
    }

    [Test]
    public async Task A_refused_pass_leaves_the_running_one_reported_untouched()
    {
        using var gate = new ReindexGate();

        var finish = new TaskCompletionSource();
        var running = gate.TryRunAsync("reindex", () => finish.Task);

        (await gate.TryRunAsync("reindex (reset)", () => Task.CompletedTask)).Should().BeFalse();

        // The refused recreate must not overwrite what the running rebuild put there.
        Status(gate).Operation.Should().Be("reindex");
        Status(gate).Running.Should().BeTrue();

        finish.SetResult();
        await running;
    }

    private static IndexStatusResponse Status(ReindexGate gate)
    {
        using var host = new IndexTestHost(gate);

        return host.Controller.Status().Result.Should().BeOfType<OkObjectResult>()
            .Subject.Value.Should().BeOfType<IndexStatusResponse>().Subject;
    }
}