using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Indexing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Pins how a recreate request survives coalescing.
/// <para>
/// The trigger collapses many requests into one rebuild, which is correct — the operation is "rebuild
/// everything". What must not collapse is the <i>intent to purge</i>. A recreate that quietly
/// degrades into a plain rebuild answers 202, runs, logs success and deletes nothing, so the stale
/// document the caller was trying to remove is still searchable with no sign anything went wrong.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexRebuildTrigger))]
public class IndexRebuildTriggerTests
{
    /// <summary>Zero, so the wait returns immediately rather than making the test time-dependent.</summary>
    private static readonly TimeSpan NoWait = TimeSpan.Zero;

    private IndexRebuildTrigger _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new IndexRebuildTrigger();

    [Test]
    public async Task A_plain_request_does_not_recreate()
    {
        _sut.Request(recreate: false);

        var wakeUp = await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        wakeUp.Requested.Should().BeTrue();
        wakeUp.Recreate.Should().BeFalse();
    }

    [Test]
    public async Task A_recreate_request_recreates()
    {
        _sut.Request(recreate: true);

        var wakeUp = await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        wakeUp.Requested.Should().BeTrue();
        wakeUp.Recreate.Should().BeTrue();
    }

    /// <summary>
    /// Both orders, because last-wins passes one of them and fails the other — a single-order test
    /// would look like cover while leaving the real case open.
    /// </summary>
    [TestCase(false, true, TestName = "Recreate_survives_when_it_arrives_last")]
    [TestCase(true, false, TestName = "Recreate_survives_when_it_arrives_first")]
    public async Task Coalescing_is_strongest_wins(bool first, bool second)
    {
        _sut.Request(recreate: first);
        _sut.Request(recreate: second);

        var wakeUp = await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        wakeUp.Recreate.Should().BeTrue(
            because: "one of the collapsed requests asked to purge, and answering 202 without "
                   + "purging reports a success that did not happen");
    }

    /// <summary>
    /// The flag is consumed, not sticky. Without the reset, one manual recreate would turn every
    /// subsequent scheduled rebuild into a destructive one for the lifetime of the process — an
    /// empty-results window every interval that nobody asked for.
    /// </summary>
    [Test]
    public async Task Recreate_is_consumed_by_the_wake_that_acts_on_it()
    {
        _sut.Request(recreate: true);
        await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        _sut.Request(recreate: false);
        var second = await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        second.Recreate.Should().BeFalse();
    }

    [Test]
    public async Task An_elapsed_interval_is_not_a_request()
    {
        var wakeUp = await _sut.WaitForRequestAsync(NoWait, CancellationToken.None);

        wakeUp.Requested.Should().BeFalse();
        wakeUp.Recreate.Should().BeFalse();
    }
}
