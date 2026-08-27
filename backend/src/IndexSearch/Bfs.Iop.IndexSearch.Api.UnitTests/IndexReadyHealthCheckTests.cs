using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Health;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Readiness must be <b>Unhealthy</b>, not Degraded, whenever the indexes cannot answer truthfully.
/// <para>
/// Degraded serialises as HTTP 200, so a Degraded readiness probe leaves the replica in rotation and
/// the endpoint becomes decoration. Only Unhealthy produces the 503 that takes it out — which is the
/// entire reason this check exists separately from the liveness one, where Degraded is correct and
/// deliberate.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexReadyHealthCheck))]
public class IndexReadyHealthCheckTests
{
    private static async Task<HealthStatus> CheckAsync(bool isReady, bool isBuilding)
    {
        var buildState = Substitute.For<IIndexBuildState>();
        buildState.IsReady.Returns(isReady);
        buildState.IsFullBuildRunning.Returns(isBuilding);

        var result = await new IndexReadyHealthCheck(buildState)
            .CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        return result.Status;
    }

    [Test]
    public async Task Healthy_only_once_a_build_has_completed_and_none_is_running()
    {
        (await CheckAsync(isReady: true, isBuilding: false)).Should().Be(HealthStatus.Healthy);
    }

    [Test]
    public async Task Unhealthy_before_the_first_build()
    {
        (await CheckAsync(isReady: false, isBuilding: false)).Should().Be(HealthStatus.Unhealthy);
    }

    /// <summary>
    /// The case this was added for: a recreate drops the indexes and then rebuilds, so a replica that
    /// was ready a moment ago is now serving nothing. <c>IsReady</c> stays true throughout — it only
    /// records that a build once finished — so the running-build check is what catches it.
    /// </summary>
    [Test]
    public async Task Unhealthy_while_a_rebuild_is_running_even_though_a_build_completed_before()
    {
        (await CheckAsync(isReady: true, isBuilding: true)).Should().Be(HealthStatus.Unhealthy);
    }
}
