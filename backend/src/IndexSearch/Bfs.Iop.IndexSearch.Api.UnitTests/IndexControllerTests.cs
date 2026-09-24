using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

[TestFixture]
internal sealed class IndexControllerTests
{
    [Test]
    public async Task A_reindex_is_accepted_rather_than_awaited()
    {
        using var host = new IndexTestHost(); 

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();

        await host.WaitForIdleAsync();
    }

    [Test]
    public async Task A_reset_reindex_builds_aside_and_swaps_the_alias()
    {
        using var host = new IndexTestHost();

        host.Controller.Reindex();

        await host.WaitForIdleAsync();

        host.Calls.Should().Contain(x => x.StartsWith("PUT /catalog-index-", StringComparison.Ordinal));
        host.Calls.Should().Contain(x => x.StartsWith("PUT /codelist-index-", StringComparison.Ordinal));
        host.Calls.Should().Contain("POST /_aliases");
    }

    [Test]
    public async Task Both_aliases_move_in_one_request()
    {
        using var host = new IndexTestHost();

        host.Controller.Reindex();

        await host.WaitForIdleAsync();

        // Two requests meant a failure between them left a new catalog paired with the previous code
        // list — and the superseded catalog already deleted, so there was no consistent pair to go
        // back to.
        host.Calls.Count(x => x == "POST /_aliases").Should().Be(1);
    }

    [Test]
    public async Task A_reset_reindex_never_drops_the_live_index()
    {
        using var host = new IndexTestHost();

        host.Controller.Reindex();

        await host.WaitForIdleAsync();

        // The whole reason for the alias. Dropping the live index first meant a failed or cancelled
        // pass left searches answering from nothing until someone noticed.
        host.Calls.Should().NotContain("DELETE /catalog-index");
        host.Calls.Should().NotContain("DELETE /codelist-index");
    }

    [Test]
    public async Task A_reset_reindex_that_fails_leaves_the_alias_where_it_was()
    {
        using var host = new IndexTestHost();

        host.CatalogSource
            .ReadAllAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("the database went away"));

        host.Controller.Reindex();

        await host.WaitForIdleAsync();

        host.Gate.LastSucceeded.Should().BeFalse();

        // Never published, and the half-built indices are cleaned up rather than left on disk.
        host.Calls.Should().NotContain("POST /_aliases");
        host.Calls.Should().Contain(x => x.StartsWith("DELETE /catalog-index-", StringComparison.Ordinal));
        host.Calls.Should().NotContain("DELETE /catalog-index");
    }

    [Test]
    public async Task A_reindex_is_refused_while_another_is_running()
    {
        using var host = new IndexTestHost();

        var running = new TaskCompletionSource();
        var held = host.Gate.TryRunAsync("reindex", () => running.Task);

        host.Controller.Reindex().Should().BeOfType<ConflictObjectResult>();

        // The point of the guard: nothing reached Elasticsearch, so the running pass still owns the
        // indices it started with.
        host.Calls.Should().BeEmpty();

        running.SetResult();
        await held;
    }

    [Test]
    public async Task A_reindex_runs_once_the_pass_holding_the_gate_has_finished()
    {
        using var host = new IndexTestHost();

        var running = new TaskCompletionSource();
        var held = host.Gate.TryRunAsync("reindex", () => running.Task);

        running.SetResult();
        await held;

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();

        await host.WaitForIdleAsync();
    }

    [Test]
    public async Task The_accepted_response_carries_the_status_to_poll()
    {
        using var host = new IndexTestHost();

        var accepted = host.Controller.Reindex()
            .Should().BeOfType<AcceptedAtActionResult>().Subject;

        accepted.ActionName.Should().Be(nameof(IndexController.Status));
        accepted.Value.Should().BeOfType<IndexStatusResponse>()
            .Subject.Running.Should().BeTrue();

        await host.WaitForIdleAsync();
    }
}
