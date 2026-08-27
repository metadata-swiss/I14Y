using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.IndexSearch.ApiClient.Health;

/// <summary>
/// Reports whether the IndexSearch service is reachable and its index usable.
/// <para>
/// Mirrors <c>IopCoreApiClientHealthCheck</c>. It exists because the forwarding path is deliberately
/// fire-and-forget: without this, Core reports perfectly healthy while every index write is being
/// dropped, and the only symptom is stale search results nobody attributes to Core.
/// </para>
/// </summary>
public sealed class IndexSearchApiClientHealthCheck : IHealthCheck
{
    private readonly IIndexSearchApiClient _apiClient;

    public IndexSearchApiClientHealthCheck(IIndexSearchApiClient apiClient) => _apiClient = apiClient;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _apiClient.GetStatusAsync(cancellationToken);

            if (status is null)
            {
                return HealthCheckResult.Unhealthy("IndexSearch returned no status payload.");
            }

            var data = new Dictionary<string, object>
            {
                ["queueDepth"] = status.QueueDepth,
                ["fullBuildRunning"] = status.FullBuildRunning,
                ["ready"] = status.Ready,
                // Surfaced but not treated as unhealthy: a missing Structures facet degrades the UI
                // without making search wrong, so it belongs in the diagnostics rather than the verdict.
                ["structuresAvailable"] = status.StructuresAvailable?.ToString() ?? "unknown",
            };

            // Not-yet-built is Degraded rather than Unhealthy: on a fresh environment the first build
            // legitimately takes a while, and failing Core's liveness for it would be wrong.
            return status.Ready
                ? HealthCheckResult.Healthy("IndexSearch reachable and its index is built.", data)
                : new HealthCheckResult(HealthStatus.Degraded, "IndexSearch reachable; its initial index build has not completed.", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("IndexSearch is unreachable.", ex);
        }
    }
}
