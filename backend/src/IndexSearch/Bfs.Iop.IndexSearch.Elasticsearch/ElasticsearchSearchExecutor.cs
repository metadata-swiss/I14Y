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

    private static string Truncate(string value) => value.Length <= 500 ? value : value[..500] + "...";
}
