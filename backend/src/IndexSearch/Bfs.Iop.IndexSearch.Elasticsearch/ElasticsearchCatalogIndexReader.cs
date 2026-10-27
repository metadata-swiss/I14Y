using System.Net;
using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogIndexReader : ICatalogIndexReader
{
    public const string HttpClientName = "IndexSearch.Elasticsearch.CarryForward";

    private readonly HttpClient _client;
    private readonly IndexNames _names;

    public ElasticsearchCatalogIndexReader(HttpClient client, IndexNames names)
    {
        _client = client;
        _names = names;
    }

    public Task<bool?> ReadStructureFlagAsync(
        Guid datasetId,
        CancellationToken cancellationToken = default) =>
        ReadFieldAsync<bool>(
            datasetId,
            EsCatalogFields.HasStructure,
            "structure flag",
            x => x.ValueKind is JsonValueKind.True or JsonValueKind.False ? x.GetBoolean() : null,
            cancellationToken);

    public Task<int?> ReadReuseCountAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        ReadFieldAsync<int>(
            id,
            EsCatalogFields.ReuseCount,
            "reuse count",
            x => x.ValueKind is JsonValueKind.Number && x.TryGetInt32(out var count) ? count : null,
            cancellationToken);

    private async Task<T?> ReadFieldAsync<T>(
        Guid id,
        string field,
        string description,
        Func<JsonElement, T?> read,
        CancellationToken cancellationToken)
        where T : struct
    {
        var path = $"/{_names.Catalog}/_doc/{id}?_source_includes={field}";

        using var response = await _client.GetAsync(path, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Reading the {description} of '{id}' failed with {(int)response.StatusCode}: {detail}");
        }

        await using var payload = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(payload, cancellationToken: cancellationToken);

        return json.RootElement.TryGetProperty("_source", out var source)
            && source.TryGetProperty(field, out var value)
            ? read(value)
            : null;
    }
}
