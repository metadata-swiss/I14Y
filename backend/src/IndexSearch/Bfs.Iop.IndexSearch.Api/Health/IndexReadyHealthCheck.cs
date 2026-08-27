using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.IndexSearch.Api.Health;

/// <summary>
/// Readiness: whether this replica should be given search traffic right now.
/// <para>
/// Deliberately separate from <see cref="ElasticsearchHealthCheck"/>, and deliberately harsher.
/// Liveness answers "is this process worth keeping alive", and reports an unbuilt index as
/// <c>Degraded</c> precisely so the platform does not kill the container mid-build in a loop.
/// Readiness answers a different question — "will a search against this replica return the truth" —
/// and while the indexes are empty the honest answer is no.
/// </para>
/// <para>
/// This matters more since the indexes are dropped before every startup build: without a readiness
/// gate, the window between the drop and the end of the build is served to users as HTTP 200 with
/// zero results, which is indistinguishable from the data having been deleted.
/// </para>
/// <para>
/// <b>It has no effect until the platform is told to use it.</b> The deploy workflows only push an
/// image; probes are part of the Container App's own configuration, and the default is a TCP check on
/// the ingress port. Until the readiness probe is pointed at <c>/health/ready</c>, this endpoint is
/// something to look at by hand, not something that removes a replica from rotation.
/// </para>
/// </summary>
internal sealed class IndexReadyHealthCheck : IHealthCheck
{
    private readonly IIndexBuildState _buildState;

    public IndexReadyHealthCheck(IIndexBuildState buildState) => _buildState = buildState;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["indexReady"] = _buildState.IsReady,
            ["fullBuildRunning"] = _buildState.IsFullBuildRunning,
        };

        if (_buildState.IsFullBuildRunning)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "A full index build is running; the indexes are incomplete.", data: data));
        }

        return Task.FromResult(_buildState.IsReady
            ? HealthCheckResult.Healthy("The indexes have been built.", data)
            : HealthCheckResult.Unhealthy("The initial index build has not completed yet.", data: data));
    }
}
