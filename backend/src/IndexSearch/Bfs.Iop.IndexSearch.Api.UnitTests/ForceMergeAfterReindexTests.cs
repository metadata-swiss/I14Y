using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// A pass rewrites every document, and the superseded copies stay on disk until a merge reclaims
// them. On an index nobody is writing to that never happens on its own — the merge policy is only
// consulted while segments flush — so without this a pass leaves its garbage behind and repeated
// passes accumulate. Measured on a real index: 543 MB of live data inside 1.65 GB.
[TestFixture]
internal sealed class ForceMergeAfterReindexTests
{
    [Test]
    public async Task A_finished_pass_collapses_the_segments_it_left()
    {
        using var host = new IndexTestHost(
            options: new IndexSearchOptions { ForceMergeAfterReindex = true });

        host.Controller.Reindex(reset: false).Should().BeOfType<AcceptedAtActionResult>();

        await host.WaitForIdleAsync();

        host.Calls.Should().Equal(
            "POST /catalog-index/_forcemerge",
            "POST /codelist-index/_forcemerge");
    }

    [Test]
    public async Task The_merge_runs_after_the_documents_are_written_not_before()
    {
        using var host = new IndexTestHost(
            options: new IndexSearchOptions { ForceMergeAfterReindex = true });

        host.Controller.Reindex(reset: true);

        await host.WaitForIdleAsync();

        // Merging first would collapse the previous generation moments before the alias moved off it,
        // and leave the new one exactly as fragmented as the pass made it. It also runs through the
        // alias, so it merges whatever the swap just published.
        host.Calls.Should().EndWith(new[]
        {
            "POST /catalog-index/_forcemerge",
            "POST /codelist-index/_forcemerge",
        });

        host.Calls.Should().Contain("POST /_aliases");
    }

    [Test]
    public async Task It_can_be_turned_off_for_a_cluster_that_has_better_things_to_do()
    {
        using var host = new IndexTestHost(
            options: new IndexSearchOptions { ForceMergeAfterReindex = false });

        host.Controller.Reindex(reset: false);

        await host.WaitForIdleAsync();

        host.Calls.Should().BeEmpty();
    }

    [Test]
    public async Task A_refused_pass_merges_nothing()
    {
        using var host = new IndexTestHost(
            options: new IndexSearchOptions { ForceMergeAfterReindex = true });

        var running = new TaskCompletionSource();
        var held = host.Gate.TryRunAsync("reindex", () => running.Task);

        host.Controller.Reindex(reset: false).Should().BeOfType<ConflictObjectResult>();

        // Force merging under a pass that is still writing is the one time it is actively wrong.
        host.Calls.Should().BeEmpty();

        running.SetResult();
        await held;
    }
}
