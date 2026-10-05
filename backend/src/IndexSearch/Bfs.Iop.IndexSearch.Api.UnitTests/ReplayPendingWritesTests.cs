using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Api.Hosting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
///     What happens to a write that arrived during a pass and then failed to replay after the swap.
/// </summary>
/// <remarks>
///     This is the least forgiving path in the feature. The journal is emptied before the replay
///     starts, and the request that produced each entry was answered long ago against the generation
///     the swap has just deleted - so if the replay drops one, nothing anywhere retries it and the
///     index disagrees with the database until the next rebuild.
/// </remarks>
[TestFixture]
internal sealed class ReplayPendingWritesTests
{
    private static readonly Guid _dataset = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public async Task A_write_that_fails_once_is_tried_again()
    {
        using var host = new IndexTestHost();

        host.Pending.Record(new PendingIndexWrite(
            PendingIndexTarget.CatalogResource,
            _dataset,
            PendingIndexOperation.Upsert,
            SearchResourceType.Dataset));

        var attempts = 0;

        host.Writer
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset, Arg.Any<CancellationToken>())
            .Returns(_ => ++attempts == 1
                ? Task.FromException(new InvalidOperationException("the shard was relocating"))
                : Task.CompletedTask);

        host.Orchestrator.TryStart().Should().BeTrue();

        await host.WaitForIdleAsync();

        attempts.Should().Be(2, "a single attempt would silently lose a write nothing else will retry");
    }

    [Test]
    public async Task A_write_that_keeps_failing_does_not_stop_the_others()
    {
        using var host = new IndexTestHost();

        var stubborn = Guid.Parse("22222222-2222-2222-2222-222222222222");

        foreach (var id in new[] { stubborn, _dataset })
        {
            host.Pending.Record(new PendingIndexWrite(
                PendingIndexTarget.CatalogResource,
                id,
                PendingIndexOperation.Upsert,
                SearchResourceType.Dataset));
        }

        host.Writer
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, stubborn, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("this one is genuinely broken"));

        host.Orchestrator.TryStart().Should().BeTrue();

        await host.WaitForIdleAsync();

        // The healthy one still landed, and the pass still reports success - the aliases moved, which
        // is what the pass was asked to do.
        await host.Writer.Received().UpsertCatalogResourceAsync(
            SearchResourceType.Dataset,
            _dataset,
            Arg.Any<CancellationToken>());

        host.Gate.LastSucceeded.Should().BeTrue();
    }

    [Test]
    public async Task The_journal_is_empty_once_the_replay_has_run()
    {
        using var host = new IndexTestHost();

        host.Pending.Record(new PendingIndexWrite(
            PendingIndexTarget.CodeList,
            _dataset,
            PendingIndexOperation.Remove));

        host.Orchestrator.TryStart().Should().BeTrue();

        await host.WaitForIdleAsync();

        // Entries left behind would be replayed again by the next pass, re-applying work that is
        // already current.
        host.Pending.Count.Should().Be(0);
    }
}
