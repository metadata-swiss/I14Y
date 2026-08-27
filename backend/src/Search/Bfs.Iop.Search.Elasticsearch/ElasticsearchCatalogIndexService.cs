using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Search.Abstractions;

using Bfs.Iop.Infrastructure.Security.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Elasticsearch-backed implementation of <see cref="ICatalogIndexService"/>, and the only one that
/// touches a search engine — every other implementation of this port forwards over HTTP.
/// Uses the low-level transport with raw JSON to stay independent of the typed query DSL.
/// </summary>
internal sealed class ElasticsearchCatalogIndexService : ICatalogIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<ElasticsearchCatalogIndexService> _logger;
    private readonly string _index;

    public ElasticsearchCatalogIndexService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        IUserContextService userContextService,
        ILogger<ElasticsearchCatalogIndexService> logger)
    {
        _client = client;
        _userContextService = userContextService;
        _logger = logger;
        _index = options.Value.CatalogIndexName;
    }

    public async Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default)
    {
        var exists = await RequestAsync(Elastic.Transport.HttpMethod.HEAD, $"/{_index}", null, cancellationToken);
        var indexExists = exists.ApiCallDetails?.HttpStatusCode == 200;

        if (indexExists && recreate)
        {
            await RequestAsync(Elastic.Transport.HttpMethod.DELETE, $"/{_index}", null, cancellationToken);
            indexExists = false;
        }

        if (!indexExists)
        {
            var create = await RequestAsync(Elastic.Transport.HttpMethod.PUT, $"/{_index}", CatalogIndexMapping.BuildCreateIndexJson(), cancellationToken);
            // Fail loudly on a bad mapping instead of letting a later bulk auto-create a
            // dynamically-mapped index (which would search incorrectly with no obvious cause).
            LogAndReadBodyOrThrow(create, $"create index '{_index}'");
        }
    }

    public async Task UpdateIndexAsync(IEnumerable<CatalogIndexEntry> entries, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var resolved = new List<CatalogIndexEntry>();

        foreach (var entry in entries)
        {
            // A null HasStructure means "keep what is indexed". The flag comes from the object store
            // rather than the database, so a caller that does not know it — a live single-dataset
            // edit, for instance — must not be able to clear it by omission. Read the current value
            // back instead of letting it default to false.
            resolved.Add(entry.Type is SearchResourceType.Dataset && entry.HasStructure is null
                ? entry with { HasStructure = await GetDatasetHasStructureValueFromIndexAsync(entry.Id, cancellationToken) }
                : entry);
        }

        await BulkIndexSafelyAsync(resolved, cancellationToken);
    }

    // Builds each document defensively: one bad entry is logged and skipped instead of aborting the
    // whole batch. A full rebuild walks the entire catalogue, so letting a single malformed resource
    // abort it would leave the index permanently short of everything after that row.
    private Task BulkIndexSafelyAsync(
        IEnumerable<CatalogIndexEntry> models,
        CancellationToken cancellationToken)
    {
        var docs = new List<(string, Dictionary<string, object?>)>();
        foreach (var model in models)
        {
            try
            {
                docs.Add(CatalogDocumentFactory.Build(model));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The object of the type '{Type}' with the id '{Id}' could not be indexed.", model.Type, model.Id);
            }
        }

        return BulkIndexAsync(docs, cancellationToken);
    }

    // Reads the current hasStructure value for a dataset from the index. Returns false when the document
    // isn't there yet (e.g. first index) — a 404 is expected here, so it is not treated as an error.
    private async Task<bool> GetDatasetHasStructureValueFromIndexAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var id = datasetId.ToString("D").ToLowerInvariant();
        var response = await RequestAsync(Elastic.Transport.HttpMethod.GET, $"/{_index}/_source/{id}", null, cancellationToken);

        if (response.ApiCallDetails?.HttpStatusCode != 200 || string.IsNullOrWhiteSpace(response.Body))
        {
            return false;
        }

        using var doc = JsonDocument.Parse(response.Body);
        return doc.RootElement.TryGetProperty(EsCatalogFields.HasStructure, out var hs)
            && hs.ValueKind is JsonValueKind.True or JsonValueKind.False
            && hs.GetBoolean();
    }

    public Task DeIndexAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var lines = new List<object>();
        foreach (var id in ids)
        {
            lines.Add(new Dictionary<string, object?>
            {
                ["delete"] = new Dictionary<string, object?> { ["_index"] = _index, ["_id"] = id.ToString("D").ToLowerInvariant() },
            });
        }

        return lines.Count == 0 ? Task.CompletedTask : BulkAsync(lines);
    }

    public async Task<PagedResult<CatalogSearchResultEntry>> SearchAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var languages = language is null ? EsCatalogFields.Languages : [language];
        var from = (page - 1) * pageSize;
        var size = pageSize == int.MaxValue ? 10000 : pageSize;

        var body = CatalogQueryBuilder.BuildSearchBody(
            queryString, languages, searchFilter,
            _userContextService.GetUserBusinessRole(), _userContextService.GetUserAgencies(), from, size);

        var response = await RequestAsync(Elastic.Transport.HttpMethod.POST, $"/{_index}/_search", Serialize(body), cancellationToken);

        return ParseSearchResponse(LogAndReadBodyOrThrow(response, "catalog search"), page, pageSize);
    }

    public async Task<IEnumerable<CatalogSearchCountResultEntry>> SearchCountAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        CancellationToken cancellationToken = default)
    {
        var languages = language is null ? EsCatalogFields.Languages : [language];

        var body = CatalogQueryBuilder.BuildCountBody(
            queryString, languages, searchFilter,
            _userContextService.GetUserBusinessRole(), _userContextService.GetUserAgencies());

        var response = await RequestAsync(Elastic.Transport.HttpMethod.POST, $"/{_index}/_search", Serialize(body), cancellationToken);

        return ParseCountResponse(LogAndReadBodyOrThrow(response, "catalog search count"));
    }

    private Task BulkIndexAsync(IEnumerable<(string Id, Dictionary<string, object?> Document)> documents, CancellationToken cancellationToken)
    {
        var lines = new List<object>();
        foreach (var (id, doc) in documents)
        {
            lines.Add(new Dictionary<string, object?>
            {
                ["index"] = new Dictionary<string, object?> { ["_index"] = _index, ["_id"] = id },
            });
            lines.Add(doc);
        }

        return lines.Count == 0 ? Task.CompletedTask : BulkAsync(lines);
    }

    private async Task BulkAsync(List<object> ndjsonLines)
    {
        var response = await _client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            "/_bulk",
            PostData.MultiJson(ndjsonLines));

        var body = EsRest.ReadBodyOrThrow(response, "bulk");
        if (EsRest.HasBulkErrors(body))
        {
            _logger.LogError("Elasticsearch bulk request reported item errors: {Body}", body);
            throw new InvalidOperationException("Elasticsearch bulk request reported item errors. See logs for details.");
        }
    }

    private Task<StringResponse> RequestAsync(Elastic.Transport.HttpMethod method, string path, string? jsonBody, CancellationToken cancellationToken) =>
        _client.Transport.RequestAsync<StringResponse>(
            method,
            path,
            jsonBody is null ? null : PostData.String(jsonBody),
            cancellationToken: cancellationToken);

    private static string Serialize(Dictionary<string, object?> body) => JsonSerializer.Serialize(body);

    // Logs and throws on an ES error response so a failed query surfaces as a real error instead of an
    // empty result list (which the parsers would otherwise produce from an error body with no "hits").
    private string LogAndReadBodyOrThrow(StringResponse response, string operation)
    {
        if (response.ApiCallDetails?.HasSuccessfulStatusCode != true)
        {
            _logger.LogError("Elasticsearch {Operation} failed (HTTP {Status}): {Body}",
                operation, response.ApiCallDetails?.HttpStatusCode, response.Body);
        }

        return EsRest.ReadBodyOrThrow(response, operation);
    }

    private static PagedResult<CatalogSearchResultEntry> ParseSearchResponse(string json, int page, int pageSize)
    {
        var results = new List<CatalogSearchResultEntry>();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("hits", out var hitsRoot))
        {
            return Empty(page, pageSize);
        }

        var total = hitsRoot.TryGetProperty("total", out var totalEl) && totalEl.TryGetProperty("value", out var totalValue)
            ? totalValue.GetInt32()
            : 0;

        if (hitsRoot.TryGetProperty("hits", out var hits))
        {
            foreach (var hit in hits.EnumerateArray())
            {
                if (hit.TryGetProperty("_source", out var src))
                {
                    results.Add(MapResult(src));
                }
            }
        }

        return new PagedResult<CatalogSearchResultEntry>
        {
            Page = page,
            PageSize = pageSize == int.MaxValue ? total : pageSize,
            Results = results,
            TotalCount = total,
        };
    }

    private static PagedResult<CatalogSearchResultEntry> Empty(int page, int pageSize) => new()
    {
        Page = page,
        PageSize = pageSize,
        Results = [],
        TotalCount = 0,
    };

    private static CatalogSearchResultEntry MapResult(JsonElement src) => new()
    {
        AccessRights = GetString(src, EsCatalogFields.AccessRights),
        BusinessEvents = GetStringArray(src, EsCatalogFields.BusinessEvents),
        ConceptType = GetInt(src, EsCatalogFields.ConceptType) is int ct ? (ConceptType)ct : null,
        CreatedAt = GetDate(src, EsCatalogFields.CreatedAt) ?? default,
        CreationType = GetInt(src, EsCatalogFields.CreationType) is int crt ? (CreationType)crt : null,
        Description = GetMultiLang(src, EsCatalogFields.Description),
        Formats = GetStringArray(src, EsCatalogFields.Formats),
        HasStructure = src.TryGetProperty(EsCatalogFields.HasStructure, out var hs) && hs.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? hs.GetBoolean()
            : null,
        Id = Guid.TryParse(GetString(src, EsCatalogFields.Id), out var id) ? id : Guid.Empty,
        Identifier = GetString(src, EsCatalogFields.Identifier) ?? string.Empty,
        LifeEvents = GetStringArray(src, EsCatalogFields.LifeEvents),
        ModifiedAt = GetDate(src, EsCatalogFields.ModifiedAt),
        PublicationLevel = GetInt(src, EsCatalogFields.PublicationLevel) is int pl ? (PublicationLevel)pl : default,
        PublicationLevelProposal = GetInt(src, EsCatalogFields.PublicationLevelProposal) is int plp ? (PublicationLevel)plp : null,
        Publisher = Guid.TryParse(GetString(src, EsCatalogFields.Publisher), out var pub) ? pub : Guid.Empty,
        RegistrationStatus = GetInt(src, EsCatalogFields.RegistrationStatus) is int rs ? (RegistrationStatus)rs : default,
        RegistrationStatusProposal = GetInt(src, EsCatalogFields.RegistrationStatusProposal) is int rsp ? (RegistrationStatus)rsp : null,
        Themes = GetStringArray(src, EsCatalogFields.Themes),
        Title = GetMultiLang(src, EsCatalogFields.Title),
        Type = Enum.TryParse<SearchResourceType>(GetString(src, EsCatalogFields.Type), out var t) ? t : SearchResourceType.Dataset,
        ValidFrom = GetDate(src, EsCatalogFields.ValidFrom),
        ValidTo = GetDate(src, EsCatalogFields.ValidTo),
        Version = GetString(src, EsCatalogFields.Version),
    };

    private static IEnumerable<CatalogSearchCountResultEntry> ParseCountResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var entries = new List<CatalogSearchCountResultEntry>();
        if (!root.TryGetProperty("aggregations", out var aggs))
        {
            return entries;
        }

        foreach (var agg in aggs.EnumerateObject())
        {
            // Each dimension is a filter aggregation wrapping the terms aggregation, so the buckets
            // live one level down. See CatalogQueryBuilder.BuildDrillSidewaysAggregations.
            if (!agg.Value.TryGetProperty(CatalogQueryBuilder.FacetValuesAggregationName, out var valuesAgg)
                || !valuesAgg.TryGetProperty("buckets", out var buckets))
            {
                continue;
            }

            var counts = new Dictionary<string, int>();
            foreach (var bucket in buckets.EnumerateArray())
            {
                var label = LabelForBucket(agg.Name, bucket);
                counts[label] = bucket.GetProperty("doc_count").GetInt32();
            }

            entries.Add(new CatalogSearchCountResultEntry
            {
                Identifier = agg.Name,
                // The filter aggregation's own doc_count: the number of documents matching every
                // selection except this dimension's. Note this is a PER-DIMENSION total, and the
                // count service reads the headline TotalDocCount from one arbitrary dimension's
                // value — so the headline number is own-filter-removed too. That is a quirk, kept
                // deliberately: "fixing" it changes a number the UI already displays.
                TotalDocumentsCount = agg.Value.TryGetProperty("doc_count", out var dimensionTotal)
                    ? dimensionTotal.GetInt32()
                    : 0,
                CountByValues = counts.AsReadOnly(),
            });
        }

        return entries;
    }

    // The numeric aggregations bucket on the integer enum value, but the count service parses each
    // key with Enum.Parse and so expects the enum NAME. Convert those keys back to names; string
    // dimensions (themes, type, …) pass through unchanged. Skip this and the facet does not come
    // back empty — it throws, taking every other facet in the response with it.
    private static string LabelForBucket(string dimension, JsonElement bucket)
    {
        // Boolean fields bucket on 0/1 with the readable form in key_as_string. HasStructure is one,
        // and the count mapping parses that label with bool.Parse, so "0" would throw.
        if (dimension == CatalogFacetDimensions.HasStructure
            && bucket.TryGetProperty("key_as_string", out var keyAsString)
            && keyAsString.ValueKind == JsonValueKind.String)
        {
            return keyAsString.GetString() ?? string.Empty;
        }

        return LabelForBucketKey(dimension, bucket.GetProperty("key"));
    }

    private static string LabelForBucketKey(string dimension, JsonElement key)
    {
        if (key.ValueKind != JsonValueKind.Number)
        {
            return key.GetString() ?? string.Empty;
        }

        var value = key.GetInt32();
        return dimension switch
        {
            CatalogFacetDimensions.RegistrationStatus or CatalogFacetDimensions.RegistrationStatusProposal
                => Enum.GetName(typeof(RegistrationStatus), value) ?? value.ToString(),
            CatalogFacetDimensions.PublicationLevel or CatalogFacetDimensions.PublicationLevelProposal
                => Enum.GetName(typeof(PublicationLevel), value) ?? value.ToString(),
            CatalogFacetDimensions.ConceptType => Enum.GetName(typeof(ConceptType), value) ?? value.ToString(),
            _ => value.ToString(),
        };
    }

    private static string? GetString(JsonElement src, string field) =>
        src.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

    private static int? GetInt(JsonElement src, string field) =>
        src.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.Number ? el.GetInt32() : null;

    private static DateTimeOffset? GetDate(JsonElement src, string field) =>
        src.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(el.GetString(), out var d)
            ? d
            : null;

    private static IEnumerable<string> GetStringArray(JsonElement src, string field)
    {
        if (!src.TryGetProperty(field, out var el) || el.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return el.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!).ToArray();
    }

    private static MultiLanguageModel GetMultiLang(JsonElement src, string field)
    {
        var dict = new Dictionary<string, string>();
        if (src.TryGetProperty(field, out var el) && el.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in el.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    dict[prop.Name] = prop.Value.GetString()!;
                }
            }
        }

        return MultiLanguageModel.FromDictionary(dict);
    }
}
