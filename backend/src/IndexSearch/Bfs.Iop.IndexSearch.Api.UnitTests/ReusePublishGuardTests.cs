using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
///     A rebuild fills a new index, so a pass that could not count reuse does not leave the indexed
///     counts alone - it produces an index with none at all, and publishing it would silently stop
///     search ordering equally relevant results.
/// </summary>
[TestFixture]
internal sealed class ReusePublishGuardTests
{
    [Test]
    public async Task A_pass_that_could_not_count_reuse_is_not_published_over_one_that_did()
    {
        using var host = new IndexTestHost();

        // The catalogue in use holds documents, so the probe finds counts on it.
        host.CatalogHolds(2936);
        Resolves(host.StructureSource);
        CannotCount(host.ReuseSource);

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();
        await host.WaitForIdleAsync();

        host.Calls.Should().NotContain("POST /_aliases");
        host.Gate.LastSucceeded.Should().BeFalse();
    }

    [Test]
    public async Task A_catalogue_that_never_ranked_on_reuse_is_not_held_back_by_it()
    {
        using var host = new IndexTestHost();

        // Nothing indexed yet, so a pass without counts takes nothing away.
        host.CatalogHolds(0);
        Resolves(host.StructureSource);
        CannotCount(host.ReuseSource);

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();
        await host.WaitForIdleAsync();

        host.Calls.Should().Contain("POST /_aliases");
        host.Gate.LastSucceeded.Should().BeTrue();
    }

    [Test]
    public async Task A_pass_that_counted_reuse_publishes_as_usual()
    {
        using var host = new IndexTestHost();

        host.CatalogHolds(2936);
        Resolves(host.StructureSource);
        host.ReuseSource.GetReuseCountsAsync(Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();
        await host.WaitForIdleAsync();

        host.Calls.Should().Contain("POST /_aliases");
        host.Gate.LastSucceeded.Should().BeTrue();
    }

    // Structures resolve throughout, so only the reuse guard can be the one refusing to publish.
    private static void Resolves(IDatasetStructureSource structures) =>
        structures.GetIdsWithStructuresAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlySet<Guid>?>(new HashSet<Guid>());

    [Test]
    public async Task A_deployment_with_nothing_to_count_reuse_with_still_publishes()
    {
        using var host = new IndexTestHost();

        host.CatalogHolds(2936);
        Resolves(host.StructureSource);
        CannotCount(host.ReuseSource);
        host.ReuseSource.IsConfigured.Returns(false);

        host.Controller.Reindex().Should().BeOfType<AcceptedAtActionResult>();
        await host.WaitForIdleAsync();

        // Such a pass can never resolve reuse, so refusing it would discard every rebuild for ever
        // and freeze the catalogue - far worse than letting a tiebreak field lapse.
        host.Calls.Should().Contain("POST /_aliases");
        host.Gate.LastSucceeded.Should().BeTrue();
    }

    // Configured, so a null result is a failed read rather than a deployment with no triple store.
    private static void CannotCount(IResourceReuseSource reuse)
    {
        reuse.IsConfigured.Returns(true);
        CannotRead(reuse);
    }

    private static void CannotRead(IResourceReuseSource reuse) =>
        reuse.GetReuseCountsAsync(Arg.Any<CancellationToken>())
            .Returns<IReadOnlyDictionary<Guid, int>?>((IReadOnlyDictionary<Guid, int>?)null);
}
