using System.Net;
using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogIndexReader : ICatalogIndexReader
{
    private readonly HttpClient _client;
    private readonly IndexNames _names;

    public ElasticsearchCatalogIndexReader(HttpClient client, IndexNames names)
    {
        _client = client;
        _names = names;
    }

    public async Task<bool?> ReadStructureFlagAsync(
        Guid datasetId,
        CancellationToken cancellationToken = default)
    {
        var path = $"/{_names.Catalog}/_doc/{datasetId}?_source_includes={EsCatalogFields.HasStructure}";

        using var response = await _client.GetAsync(path, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Reading the structure flag of '{datasetId}' failed with {(int)response.StatusCode}: {detail}");
        }

        await using var payload = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(payload, cancellationToken: cancellationToken);

        return json.RootElement.TryGetProperty("_source", out var source)
            && source.TryGetProperty(EsCatalogFields.HasStructure, out var flag)
            && flag.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? flag.GetBoolean()
            : null;
    }
}
