using System.Net;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.IndexSearch.ApiClient;

/// <inheritdoc cref="IIndexSearchApiClient"/>
internal sealed class IndexSearchApiClient : IIndexSearchApiClient
{
    /// <summary>
    /// Must match the IndexSearch service's own serializer settings. Without the enum converter,
    /// values such as <c>SearchResourceType</c> serialise as integers and the receiving side rejects
    /// the payload — a failure that reads like a data problem rather than a wiring one.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _httpClient;

    public IndexSearchApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task IndexCatalogAsync(IReadOnlyCollection<CatalogIndexEntry> entries, CancellationToken cancellationToken = default) =>
        PostAsync("api/Index/catalog", entries, cancellationToken);

    public Task IndexCodeListEntriesAsync(IReadOnlyCollection<CodeListIndexEntry> entries, CancellationToken cancellationToken = default) =>
        PostAsync("api/Index/codelist-entries", entries, cancellationToken);

    public Task DeIndexCatalogAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        DeleteAsync("api/Index/catalog", ids, cancellationToken);

    public Task DeIndexCodeListEntriesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        DeleteAsync("api/Index/codelist-entries", ids, cancellationToken);

    public async Task<IndexSearchStatus?> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("api/Index", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IndexSearchStatus>(_jsonOptions, cancellationToken);
    }

    private async Task PostAsync<T>(string route, IReadOnlyCollection<T> models, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(models);

        if (models.Count == 0)
        {
            return;
        }

        using var response = await _httpClient.PostAsJsonAsync(route, models, _jsonOptions, cancellationToken);

        EnsureAccepted(response, route);
    }

    private async Task DeleteAsync(string route, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count == 0)
        {
            return;
        }

        // HttpClient has no DeleteAsJsonAsync; the ids travel in the body because a long id list does
        // not belong in a URL.
        using var request = new HttpRequestMessage(HttpMethod.Delete, route)
        {
            Content = JsonContent.Create(ids, options: _jsonOptions),
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        EnsureAccepted(response, route);
    }

    private static void EnsureAccepted(HttpResponseMessage response, string route)
    {
        // Distinguish the two failures worth telling apart in a log: a rejected secret is a
        // configuration mistake that will never fix itself, while a full queue is transient
        // backpressure. Both otherwise surface as an opaque "request failed".
        if (response.StatusCode is HttpStatusCode.Forbidden)
        {
            throw new HttpRequestException(
                $"IndexSearch rejected {route} with 403. The configured secret does not match the " +
                "service's IndexSearch:Secret, so no index write will ever succeed.",
                inner: null,
                HttpStatusCode.Forbidden);
        }

        if (response.StatusCode is HttpStatusCode.TooManyRequests)
        {
            throw new HttpRequestException(
                $"IndexSearch rejected {route} with 429: its ingest queue is full.",
                inner: null,
                HttpStatusCode.TooManyRequests);
        }

        response.EnsureSuccessStatusCode();
    }
}
