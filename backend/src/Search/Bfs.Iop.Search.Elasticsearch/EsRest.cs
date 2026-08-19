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
}
