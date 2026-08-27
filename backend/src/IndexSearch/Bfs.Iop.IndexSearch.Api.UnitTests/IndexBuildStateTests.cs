using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Indexing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// The two properties of <see cref="IndexBuildState"/> that other components trust and cannot verify
/// for themselves: that readiness stops being true when the documents go, and that a reconcile and a
/// rebuild of the same index never overlap.
/// </summary>
[TestFixture(TestOf = typeof(IndexBuildState))]
public class IndexBuildStateTests
{
    private IndexBuildState _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new IndexBuildState();

    private Task Build(bool? structuresAvailable = true) =>
        _sut.RunFullBuildAsync(_ => Task.FromResult(structuresAvailable), CancellationToken.None);

    [Test]
    public async Task A_completed_build_makes_the_index_ready()
    {
        await Build();

        _sut.IsReady.Should().BeTrue();
        _sut.LastFullBuildCompletedAt.Should().NotBeNull();
    }

    /// <summary>
    /// The defect this exists for. <c>IsReady</c> latches on the first success and no failure clears
    /// it — correct for a plain rebuild, where the old documents survive a failure, and wrong for a
    /// recreate, where they are deleted before the rebuild starts. Without an explicit signal, a
    /// recreate that dies mid-build reports <c>ready: true</c> over an empty index, and the readiness
    /// probe keeps the replica serving it.
    /// </summary>
    [Test]
    public async Task MarkNotReady_withdraws_readiness_that_an_earlier_build_had_earned()
    {
        await Build();
        _sut.IsReady.Should().BeTrue(because: "the fixture is only meaningful if readiness was earned first");

        _sut.MarkNotReady();

        _sut.IsReady.Should().BeFalse();
        _sut.LastFullBuildCompletedAt.Should().BeNull();
    }

    /// <summary>
    /// The structures answer described the documents that have just been deleted, so it cannot
    /// outlive them — otherwise GET /api/Index reports a Structures verdict for an index that no
    /// longer contains the datasets it was measured against.
    /// </summary>
    [Test]
    public async Task MarkNotReady_also_withdraws_the_structures_verdict()
    {
        await Build(structuresAvailable: true);
        _sut.StructuresAvailable.Should().BeTrue();

        _sut.MarkNotReady();

        _sut.StructuresAvailable.Should().BeNull();
    }

    /// <summary>
    /// A failing plain rebuild must NOT withdraw readiness: the previously indexed documents are
    /// still there and still serve correctly. This is the other half of the contract, and pinning it
    /// stops the fix above from being "solved" by clearing the latch on every failure.
    /// </summary>
    [Test]
    public async Task A_failed_build_leaves_an_earlier_success_intact()
    {
        await Build();

        var act = async () => await _sut.RunFullBuildAsync(
            _ => Task.FromException<bool?>(new InvalidOperationException("boom")),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        _sut.IsReady.Should().BeTrue();
    }

    /// <summary>
    /// Gated work must hold the gate, not merely wait for it to be free. A writer that only checks
    /// first can still be mid-flight when the builder drops the index, and its bulk request then makes
    /// Elasticsearch auto-create the index with a dynamic mapping — searching wrongly, silently.
    /// </summary>
    [Test]
    public async Task Gated_work_and_a_full_build_cannot_overlap()
    {
        var buildStarted = new TaskCompletionSource();
        var releaseBuild = new TaskCompletionSource();
        var gatedWorkRan = false;

        var build = _sut.RunFullBuildAsync(
            async _ =>
            {
                buildStarted.SetResult();
                await releaseBuild.Task;
                return true;
            },
            CancellationToken.None);

        await buildStarted.Task;

        var gated = _sut.RunGatedAsync(
            _ => { gatedWorkRan = true; return Task.CompletedTask; },
            CancellationToken.None);

        gatedWorkRan.Should().BeFalse(because: "the build holds the gate, so the write must not have started");

        releaseBuild.SetResult();
        await build;
        await gated;

        gatedWorkRan.Should().BeTrue(because: "once the build releases the gate the write proceeds");
    }

    /// <summary>
    /// The direction that actually matters, and the one the obvious test misses.
    /// <para>
    /// A gate that is merely <i>waited on</i> and released before the work — the shape this replaced —
    /// passes <see cref="Gated_work_and_a_full_build_cannot_overlap"/> unchanged, because waiting
    /// still blocks while a build holds it. What it does not do is stop a build starting once the
    /// write is already in flight, which is exactly the sequence that lets the builder drop the index
    /// underneath a bulk request and have Elasticsearch auto-create it with a dynamic mapping.
    /// </para>
    /// </summary>
    [Test]
    public async Task A_full_build_cannot_start_while_gated_work_is_in_flight()
    {
        var workStarted = new TaskCompletionSource();
        var releaseWork = new TaskCompletionSource();
        var buildRan = false;

        var gated = _sut.RunGatedAsync(
            async _ =>
            {
                workStarted.SetResult();
                await releaseWork.Task;
            },
            CancellationToken.None);

        await workStarted.Task;

        var build = _sut.RunFullBuildAsync(
            _ => { buildRan = true; return Task.FromResult<bool?>(true); },
            CancellationToken.None);

        buildRan.Should().BeFalse(
            because: "the reconcile holds the gate, so a rebuild must not begin — releasing the gate "
                   + "before the write is what lets a rebuild drop the index underneath it");

        releaseWork.SetResult();
        await gated;
        await build;

        buildRan.Should().BeTrue(because: "the rebuild proceeds once the write has finished");
    }

    /// <summary>
    /// A reconcile is not a rebuild. Reporting it as one would make POST /api/Index/rebuild and
    /// /recreate answer 409 whenever the event queue happened to be draining.
    /// </summary>
    [Test]
    public async Task Gated_work_is_not_reported_as_a_full_build()
    {
        var observed = true;

        await _sut.RunGatedAsync(
            _ => { observed = _sut.IsFullBuildRunning; return Task.CompletedTask; },
            CancellationToken.None);

        observed.Should().BeFalse();
    }

    /// <summary>The gate is released even when the gated work throws, or the service deadlocks.</summary>
    [Test]
    public async Task A_throwing_gated_write_still_releases_the_gate()
    {
        var act = async () => await _sut.RunGatedAsync(
            _ => Task.FromException(new InvalidOperationException("boom")),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        // Would hang rather than fail if the gate leaked, so the timeout is the assertion.
        var second = _sut.RunGatedAsync(_ => Task.CompletedTask, CancellationToken.None);

        (await Task.WhenAny(second, Task.Delay(TimeSpan.FromSeconds(5))))
            .Should().Be(second, because: "a leaked gate would stall every later reconcile");
    }
}
