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
            ["id"] = metadata.Id.ToString(),
            ["resourceType"] = metadata.ResourceType.ToString()
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

    public async Task<IEnumerable<Commit>> GetCommitsAsync(CommitSearchFilters filters, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filters, nameof(filters));

        var query = new List<KeyValuePair<string, string?>>();

        if (!string.IsNullOrWhiteSpace(filters.Author))
        {
            query.Add(new(nameof(filters.Author), filters.Author));
        }

        if (filters.From.HasValue)
        {
            query.Add(new(nameof(filters.From), filters.From.ToString()));
        }

        if (filters.ResourceId.HasValue)
        {
            query.Add(new(nameof(filters.ResourceId), filters.ResourceId.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(filters.ResourceType))
        {
            query.Add(new(nameof(filters.ResourceType), filters.ResourceType));
        }

        if (filters.To.HasValue)
        {
            query.Add(new(nameof(filters.To), filters.To.ToString()));
        }

        var url = QueryHelpers.AddQueryString("audittrail/commits", query);

        var commits = await _httpClient.GetFromJsonAsync<IEnumerable<Commit>>(url, cancellationToken);
        return commits ?? [];
    }
}