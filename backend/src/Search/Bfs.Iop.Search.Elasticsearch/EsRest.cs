using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Thin helpers over the Elasticsearch low-level transport (raw JSON). Keeps the index/search services
/// independent of the strongly-typed query DSL and makes the emitted JSON copy-pasteable into Kibana.
/// </summary>
internal static class EsRest
{
    public static Task<StringResponse> SendAsync(
        ElasticsearchClient client,
        Elastic.Transport.HttpMethod method,
        string path,
        string? jsonBody = null,
        CancellationToken cancellationToken = default) =>
        client.Transport.RequestAsync<StringResponse>(
            method,
            path,
            jsonBody is null ? null : PostData.String(jsonBody),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Returns the response body for a successful call, or throws with the ES status + reason on failure.
    /// Prevents an error response (which has no <c>hits</c>) from being silently parsed as "0 results".
    /// </summary>
    public static string ReadBodyOrThrow(StringResponse response, string operation)
    {
        if (response.ApiCallDetails?.HasSuccessfulStatusCode == true)
        {
            return response.Body;
        }

        var status = response.ApiCallDetails?.HttpStatusCode;
        var body = response.Body ?? string.Empty;
        if (body.Length > 2000)
        {
            body = body[..2000] + "…";
        }

        throw new InvalidOperationException($"Elasticsearch {operation} failed (HTTP {status}): {body}");
    }

    public static Task<StringResponse> BulkAsync(ElasticsearchClient client, List<object> ndjsonLines) =>
        client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            "/_bulk",
            PostData.MultiJson(ndjsonLines));

    /// <summary>
    /// True when a bulk response reports per-item failures. The transport call can return HTTP 200 while
    /// individual actions failed (mapping conflicts, etc.); those are only visible via the top-level
    /// <c>"errors"</c> flag, so checking the HTTP status alone would let them pass silently. Parses the
    /// JSON rather than substring-matching, and is conservative on a parse failure (an unparseable body on
    /// an otherwise-successful bulk call is itself suspicious → report it).
    /// </summary>
    public static bool HasBulkErrors(string? body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("errors", out var errors)
                && errors.ValueKind == JsonValueKind.True;
        }
        catch (JsonException)
        {
            return true;
        }
    }

    public static async Task EnsureIndexAsync(
        ElasticsearchClient client,
        string index,
        string createBodyJson,
        bool recreate,
        CancellationToken cancellationToken = default)
    {
        var head = await SendAsync(client, Elastic.Transport.HttpMethod.HEAD, $"/{index}", null, cancellationToken);
        var exists = head.ApiCallDetails?.HttpStatusCode == 200;

        if (exists && recreate)
        {
            await SendAsync(client, Elastic.Transport.HttpMethod.DELETE, $"/{index}", null, cancellationToken);
            exists = false;
        }

        if (!exists)
        {
            var create = await SendAsync(client, Elastic.Transport.HttpMethod.PUT, $"/{index}", createBodyJson, cancellationToken);
            // Fail loudly on a bad mapping instead of letting a later bulk auto-create a
            // dynamically-mapped index (which would search incorrectly with no obvious cause).
            ReadBodyOrThrow(create, $"create index '{index}'");
        }
    }

    /// <summary>
    /// Slows automatic refresh right down for the duration of a bulk load. Disposing restores the
    /// cluster default and makes everything written in the meantime searchable immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Elasticsearch refreshes once a second by default, so a long rebuild cuts a new segment every
    /// second and then has to merge them all. Stretching the interval removes almost all of that.
    /// </para>
    /// <para>
    /// <b>Stretched to <see cref="BulkLoadRefreshInterval"/> rather than disabled with <c>-1</c>, and
    /// the difference matters more than the small amount of throughput it costs.</b> With refresh
    /// off, an index under construction reports <c>0</c> documents for the whole rebuild while its
    /// store size climbs — indistinguishable from a broken build to anyone watching, and it reliably
    /// convinces people that indexing has failed. Worse, <c>-1</c> survives the process: kill the
    /// host mid-build — stopping a debug session does exactly this — and <see cref="DisposeAsync"/>
    /// never runs, leaving the index permanently unable to surface anything written to it. A stale
    /// interval of a few seconds is a harmless leftover; <c>-1</c> is a silent outage.
    /// </para>
    /// <para>
    /// The throughput given up is negligible: over a rebuild of several minutes this is a handful of
    /// refreshes instead of hundreds.
    /// </para>
    /// </remarks>
    public static async Task<IAsyncDisposable> SuspendRefreshAsync(
        ElasticsearchClient client,
        string index,
        CancellationToken cancellationToken = default)
    {
        await SetRefreshIntervalAsync(client, index, BulkLoadRefreshInterval, cancellationToken);

        return new RefreshRestorer(client, index);
    }

    /// <summary>
    /// Refresh interval held during a full rebuild. Long enough that refreshing costs almost nothing,
    /// short enough that progress is visibly moving and a crashed build leaves nothing worse than a
    /// slightly stale index.
    /// </summary>
    private const string BulkLoadRefreshInterval = "30s";

    private static Task<StringResponse> SetRefreshIntervalAsync(
        ElasticsearchClient client,
        string index,
        string? interval,
        CancellationToken cancellationToken) =>
        SendAsync(
            client,
            Elastic.Transport.HttpMethod.PUT,
            $"/{index}/_settings",
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["index"] = new Dictionary<string, object?> { ["refresh_interval"] = interval },
            }),
            cancellationToken);

    private sealed class RefreshRestorer : IAsyncDisposable
    {
        private readonly ElasticsearchClient _client;
        private readonly string _index;

        public RefreshRestorer(ElasticsearchClient client, string index)
        {
            _client = client;
            _index = index;
        }

        public async ValueTask DisposeAsync()
        {
            // Deliberately not CancellationToken.None by accident: the restore has to run even when
            // the build was cancelled, because a cancelled build is exactly the case that would
            // otherwise leave the index refreshing far more slowly than the rest of its life.
            //
            // null resets the setting to the cluster default rather than pinning it at "1s".
            await SetRefreshIntervalAsync(_client, _index, null, CancellationToken.None);

            // Without this, everything written during the build waits up to the stretched interval
            // before it can be searched, so a rebuild that has just reported success would still
            // answer queries from the state before it ran.
            await SendAsync(_client, Elastic.Transport.HttpMethod.POST, $"/{_index}/_refresh", null, CancellationToken.None);
        }
    }
}
