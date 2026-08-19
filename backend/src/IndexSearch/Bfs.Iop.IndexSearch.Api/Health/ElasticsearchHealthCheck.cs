using Bfs.Iop.IndexSearch.Api.Indexing;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.IndexSearch.Api.Health;

/// <summary>
/// Liveness of the Elasticsearch dependency, plus index readiness as data rather than as a failure.
/// <para>
/// Reporting "not built yet" as Unhealthy would make the platform kill the container mid-build and
/// loop forever, so the first build is reported as Degraded and surfaced through GET /api/Index.
/// </para>
/// </summary>
internal sealed class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly ElasticsearchClient _client;
    private readonly IIndexBuildState _buildState;

    public ElasticsearchHealthCheck(ElasticsearchClient client, IIndexBuildState buildState)
    {
        _client = client;
        _buildState = buildState;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.PingAsync(cancellationToken);

            if (!response.IsValidResponse)
            {
                return HealthCheckResult.Unhealthy("Elasticsearch did not answer the ping.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Elasticsearch is unreachable.", ex);
        }

        var data = new Dictionary<string, object>
        {
            ["indexReady"] = _buildState.IsReady,
            ["fullBuildRunning"] = _buildState.IsFullBuildRunning,
        };

        return _buildState.IsReady
            ? HealthCheckResult.Healthy("Elasticsearch reachable and the index has been built.", data)
            : new HealthCheckResult(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, "Elasticsearch reachable; the initial index build has not completed yet.", data: data);
    }
}
