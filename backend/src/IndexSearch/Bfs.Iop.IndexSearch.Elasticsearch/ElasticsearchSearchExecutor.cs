using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchSearchExecutor
{
    public const string HttpClientName = "IndexSearch.Elasticsearch.Search";

    private readonly HttpClient _client;

    public ElasticsearchSearchExecutor(HttpClient client)
    {
        _client = client;
    }

    public async Task<JsonDocument> SearchAsync(
        string index,
        Dictionary<string, object?> body,
        CancellationToken cancellationToken)
    {
        using var response = await _client.PostAsJsonAsync($"/{index}/_search", body, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"A search of '{index}' failed with {(int)response.StatusCode}: {Truncate(detail)}");
        }

        await using var payload = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonDocument.ParseAsync(payload, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     Opens a point-in-time view of an index and returns its id.
    ///     <para>
    ///         Deep paging by page number stops at <c>index.max_result_window</c>. A point in time plus
    ///         <c>search_after</c> has no such ceiling, and it freezes the index so a long walk cannot
    ///         see the same entry twice or miss one because a reindex swapped the alias underneath it.
    ///     </para>
    /// </summary>
    public async Task<string> OpenPointInTimeAsync(
        string index,
        TimeSpan keepAlive,
        CancellationToken cancellationToken)
    {
        using var response = await _client.PostAsync(
            $"/{index}/_pit?keep_alive={KeepAlive(keepAlive)}",
            content: null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Opening a point in time on '{index}' failed with {(int)response.StatusCode}: {Truncate(detail)}");
        }

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        return payload.RootElement.GetProperty("id").GetString()
            ?? throw new HttpRequestException($"Elasticsearch opened a point in time on '{index}' without an id.");
    }

    /// <summary>Searches a point in time. The index is carried by the view, not the path.</summary>
    public async Task<JsonDocument> SearchPointInTimeAsync(
        Dictionary<string, object?> body,
        CancellationToken cancellationToken)
    {
        using var response = await _client.PostAsJsonAsync("/_search", body, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"A point in time search failed with {(int)response.StatusCode}: {Truncate(detail)}");
        }

        await using var payload = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonDocument.ParseAsync(payload, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     Closes a point in time. Best effort: the view expires on its own keep-alive, so a failure
    ///     here costs a little memory on the cluster for a while and must not fail the caller's work.
    /// </summary>
    public async Task ClosePointInTimeAsync(string id, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/_pit")
        {
            Content = JsonContent.Create(new Dictionary<string, object?> { ["id"] = id }),
        };

        try
        {
            using var response = await _client.SendAsync(request, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            // Swallowed deliberately. An HttpClient timeout arrives as TaskCanceledException rather
            // than HttpRequestException, and the caller passes CancellationToken.None precisely so
            // that nothing here can cancel - so a cancellation at this point is a timeout, and
            // letting it out would turn a finished export into a failed one.
        }
    }

    private static string KeepAlive(TimeSpan keepAlive) =>
        $"{Math.Max(1, (int)Math.Ceiling(keepAlive.TotalSeconds))}s";

    private static string Truncate(string value) => value.Length <= 500 ? value : value[..500] + "...";
}
