using Bfs.Iop.AuditTrail.Abstractions.Models;
using System.Net.Http.Json;

namespace Bfs.Iop.AuditTrail.ApiClient;

internal sealed class AuditTrailApiClient : IAuditTrailApiClient
{
    private readonly HttpClient _httpClient;

    public AuditTrailApiClient(HttpClient httpClient) =>
        _httpClient = httpClient;

    public async Task<bool> GetRepositoryExistsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("audittrail/repository-exists", cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return bool.Parse(content);
    }

    public async Task InitRepositoryAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("audittrail/repository-init", cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("audittrail/resource-tracked?dataFormat=" + metadata.DataFormat + "&id=" + metadata.Id + "&identifier=" + metadata.Identifier + "&type=" + metadata.Type, cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return bool.Parse(content);
    }

    public async Task CommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync("audittrail/commit", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
