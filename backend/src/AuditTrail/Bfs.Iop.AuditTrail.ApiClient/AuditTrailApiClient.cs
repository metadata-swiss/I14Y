using Bfs.Iop.AuditTrail.Abstractions.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Json;

namespace Bfs.Iop.AuditTrail.ApiClient;

internal sealed class AuditTrailApiClient : IAuditTrailApiClient
{
    private readonly HttpClient _httpClient;

    public AuditTrailApiClient(HttpClient httpClient) =>
        _httpClient = httpClient;

    public async Task<bool> RepositoryExistsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("audittrail/repository-exists", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }

    public async Task InitRepositoryAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync("audittrail/repository-init", null, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var query = new Dictionary<string, string?>
        {
            ["dataFormat"] = metadata.DataFormat,
            ["id"] = metadata.Id.ToString(),
            ["identifier"] = metadata.Identifier,
            ["type"] = metadata.Type
        };

        var uri = QueryHelpers.AddQueryString("audittrail/resource-tracked", query);

        var response = await _httpClient.GetAsync(uri, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }

    public async Task CommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _httpClient.PostAsJsonAsync("audittrail/commit", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}