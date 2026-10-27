using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;
 
[TestFixture]
internal sealed class ForceMergeAfterReindexTests
{
    [Test]
    public async Task A_finished_pass_collapses_the_segments_it_left()
    {
        using var host = new IndexTestHost();

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();

        await host.WaitForIdleAsync();
 
        host.Calls.Should().EndWith(new[]
        {
            "POST /catalog-index/_forcemerge",
            "POST /codelist-index/_forcemerge",
        });
    }

    [Test]
    public async Task The_merge_runs_after_the_documents_are_written_not_before()
    {
        using var host = new IndexTestHost();

        host.Controller.Reindex();

        await host.WaitForIdleAsync(); 

        host.Calls.Should().EndWith(new[]
        {
            "POST /catalog-index/_forcemerge",
            "POST /codelist-index/_forcemerge",
        });

        host.Calls.Should().Contain("POST /_aliases");
    }

    [Test]
    public async Task A_refused_pass_merges_nothing()
    {
        using var host = new IndexTestHost();

        var running = new TaskCompletionSource();
        var held = host.Gate.TryRunAsync("reindex", () => running.Task);

        host.Controller.Reindex().Should().BeOfType<ConflictObjectResult>();

        // Force merging under a pass that is still writing is the one time it is actively wrong.
        host.Calls.Should().BeEmpty();

        running.SetResult();
        await held;
    }
}
