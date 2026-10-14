using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bfs.Iop.IndexSearch.ApiClient.Health;

public sealed class IndexSearchApiClientHealthCheck : IHealthCheck
{
    private readonly IIndexSearchApiClient _apiClient;

    public IndexSearchApiClientHealthCheck(IIndexSearchApiClient apiClient) => _apiClient = apiClient;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.GetSearchCatalogByQueryAndLanguageAndPageAndPageSizeAsync(
                query: null,
                language: null,
                page: 1,
                pageSize: 1,
                cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
