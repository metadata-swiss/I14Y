using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Api.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
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

    public async Task<PagedResult<Commit>> GetCommitsAsync(
        CommitSearchFilters filters,
        int? page = null,
        int? pageSize = null, 
        CancellationToken cancellationToken = default)
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

        if (page.HasValue)
        {
            query.Add(new(nameof(page), page.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (pageSize.HasValue)
        {
            query.Add(new(nameof(pageSize), pageSize.Value.ToString(CultureInfo.InvariantCulture)));
        }

        var url = QueryHelpers.AddQueryString("audittrail/commits", query);

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var commits = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Commit>>(cancellationToken)
            ?? [];

        return new PagedResult<Commit>
        {
            Results = commits,
            Page = TryGetIntHeader(response, HttpContextExtensions.PageHeaderKey) ?? page ?? 1,
            PageSize = TryGetIntHeader(response, HttpContextExtensions.PageSizeHeaderKey) ?? pageSize ?? commits.Count,
            TotalCount = TryGetIntHeader(response, HttpContextExtensions.TotalRowsHeaderKey) ?? commits.Count
        };
    }

    private static int? TryGetIntHeader(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values)
        && int.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}