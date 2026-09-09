using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchBulkWriter
{
    public const string HttpClientName = "IndexSearch.Elasticsearch";

    private const string NewlineDelimitedJson = "application/x-ndjson";

    private readonly HttpClient _client;
    private readonly ILogger<ElasticsearchBulkWriter> _logger;

    public ElasticsearchBulkWriter(HttpClient client, ILogger<ElasticsearchBulkWriter> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<int> IndexAsync(
        string index,
        IReadOnlyCollection<IndexRequest> documents,
        CancellationToken cancellationToken) =>
        SendAsync(index, BuildIndexBody(documents), documents.Count, "index", cancellationToken);

    public Task<int> DeleteAsync(
        string index,
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        SendAsync(index, BuildDeleteBody(ids), ids.Count, "delete", cancellationToken);

    private static string BuildIndexBody(IReadOnlyCollection<IndexRequest> documents)
    {
        var body = new StringBuilder();

        foreach (var (id, document) in documents)
        {
            AppendAction(body, "index", id);
            body.Append(JsonSerializer.Serialize(document)).Append('\n');
        }

        return body.ToString();
    }

    private static string BuildDeleteBody(IReadOnlyCollection<Guid> ids)
    {
        var body = new StringBuilder();

        foreach (var id in ids)
        {
            AppendAction(body, "delete", id.ToString());
        }

        return body.ToString();
    }

    private static void AppendAction(StringBuilder body, string action, string id)
    {
        // Newline delimited, and the trailing newline is not optional: Elasticsearch rejects a bulk
        // body whose last line is unterminated.
        body
            .Append("{\"")
            .Append(action)
            .Append("\":{\"_id\":")
            .Append(JsonSerializer.Serialize(id))
            .Append("}}\n");
    }

    private async Task<int> SendAsync(
        string index,
        string body,
        int requested,
        string action,
        CancellationToken cancellationToken)
    {
        if (requested == 0)
        {
            return 0;
        }

        using var content = new StringContent(body, Encoding.UTF8, NewlineDelimitedJson);

        var response = await _client.PostAsync($"/{index}/_bulk", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // The whole request failed, so nothing landed. Throwing lets the caller count the batch as
            // lost rather than silently reporting zero written.
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"A bulk {action} of {requested} documents into '{index}' failed with "
                + $"{(int)response.StatusCode}: {Truncate(detail)}");
        }

        return CountAccepted(await response.Content.ReadAsStringAsync(cancellationToken), index, action, requested);
    }

    private int CountAccepted(string payload, string index, string action, int requested)
    {
        using var document = JsonDocument.Parse(payload);

        if (!document.RootElement.TryGetProperty("items", out var items))
        {
            _logger.LogError("A bulk {Action} into {Index} returned no items array.", action, index);
            return 0;
        }

        var accepted = 0;
        var firstError = (string?)null;

        foreach (var item in items.EnumerateArray())
        {
            foreach (var operation in item.EnumerateObject())
            {
                var status = operation.Value.TryGetProperty("status", out var s) ? s.GetInt32() : 0;

                if (status is >= 200 and < 300)
                {
                    accepted++;
                }
                else if (firstError is null && operation.Value.TryGetProperty("error", out var error))
                {
                    firstError = error.ToString();
                }
            }
        }

        if (accepted < requested)
        {
            // One example rather than thousands: a rejected batch usually fails the same way for every
            // document in it, and the count is what the caller acts on.
            _logger.LogError(
                "Elasticsearch accepted {Accepted} of {Requested} documents in a bulk {Action} into "
                + "{Index}. First rejection: {Error}",
                accepted,
                requested,
                action,
                index,
                firstError ?? "not reported");
        }

        return accepted;
    }

    private static string Truncate(string value) =>
        value.Length <= 500 ? value : value[..500] + "...";
}
