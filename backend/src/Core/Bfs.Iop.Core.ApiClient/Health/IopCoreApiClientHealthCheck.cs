using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.ApiClient.Health;

public sealed class IopCoreApiClientHealthCheck : IHealthCheck
{
    private readonly IIopCoreApiClient _apiClient;

    public IopCoreApiClientHealthCheck(IIopCoreApiClient apiClient) => _apiClient = apiClient;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.GetUsersCurrentAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
