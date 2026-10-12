using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>What the service does with the indices when it starts.</summary>
[TestFixture]
internal sealed class IndexStartupTests
{
    [Test]
    public async Task An_empty_catalogue_is_filled_rather_than_left_until_the_next_slot()
    {
        using var host = new IndexTestHost();

        await Startup(host).StartAsync(CancellationToken.None);

        await host.WaitForIdleAsync();

        host.Gate.Operation.Should().Be("reindex", "startup should have run a pass, not only created the indices");
        host.Gate.LastSucceeded.Should().BeTrue();
    }

    [Test]
    public async Task A_catalogue_that_already_holds_documents_is_left_alone()
    {
        using var host = new IndexTestHost();

        host.CatalogHolds(2_954);

        await Startup(host).StartAsync(CancellationToken.None);

        // A full pass on every rollout, for an index that is already serving, is the thing to avoid.
        host.Gate.IsRunning.Should().BeFalse();
        host.Gate.Operation.Should().BeNull();
        host.Gate.LastSucceeded.Should().BeNull();
    }

    private static IndexStartupService Startup(IndexTestHost host) =>
        new(
            host.Services.GetRequiredService<IServiceScopeFactory>(),
            host.Orchestrator,
            NullLogger<IndexStartupService>.Instance);
}
