using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.IndexSearch.Api.Health;

internal sealed class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly ElasticsearchIndexProvisioner _provisioner;
    private readonly IndexNames _names;

    public ElasticsearchHealthCheck(ElasticsearchIndexProvisioner provisioner, IndexNames names)
    {
        _provisioner = provisioner;
        _names = names;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var missing = new List<string>();

            foreach (var index in new[] { _names.Catalog, _names.CodeList })
            {
                if (!await _provisioner.ExistsAsync(index, cancellationToken))
                {
                    missing.Add(index);
                }
            }

            return missing.Count == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Missing {string.Join(" and ", missing)}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Elasticsearch could not be reached.", ex);
        }
    }
}
