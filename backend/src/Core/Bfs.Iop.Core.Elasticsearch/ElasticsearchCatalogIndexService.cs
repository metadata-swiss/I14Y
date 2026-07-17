using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Infrastructure.Security.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Elasticsearch;

/// <summary>
/// Elasticsearch-backed implementation of <see cref="ICatalogIndexService"/>. Drop-in alternative to
/// the Lucene <c>CatalogIndexService</c>: the MediatR command handlers depend only on the interface,
/// so switching engines is a DI-registration change (see <c>Search:Engine</c> in appsettings).
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
            ReadBodyOrLog(create, $"create index '{_index}'");
        }
    }

    public void UpdateIndex(DcatDatasetModel model, bool? hasStructure = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        BulkIndex([CatalogDocumentFactory.FromDataset(model, hasStructure ?? false)]);
    }

    public void UpdateIndex(IEnumerable<DcatDatasetModel> models, IEnumerable<string> datasetsStructuresFileNames)
    {
        ArgumentNullException.ThrowIfNull(models);
        var ids = datasetsStructuresFileNames.ToArray();
        BulkIndex(models.Select(m =>
            CatalogDocumentFactory.FromDataset(m, ids.Any(x => x.StartsWith(m.Id.ToString())))));
    }

    public void UpdateIndex(params PublicServiceModel[] models) =>
        BulkIndex(models.Select(CatalogDocumentFactory.FromPublicService));

    public void UpdateIndex(params DataServiceModel[] models) =>
        BulkIndex(models.Select(CatalogDocumentFactory.FromDataService));

    public void UpdateIndex(params IopConceptModel[] models) =>
        BulkIndex(models.Select(CatalogDocumentFactory.FromConcept));

    public void UpdateIndex(params MappingTableModel[] models) =>
        BulkIndex(models.Select(CatalogDocumentFactory.FromMappingTable));

    public void DeIndex(params Guid[] ids)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Length == 0)
        {
            return;
        }

        var lines = new List<object>();
        foreach (var id in ids)
        {
            lines.Add(new Dictionary<string, object?>
            {
                ["delete"] = new Dictionary<string, object?> { ["_index"] = _index, ["_id"] = id.ToString("D").ToLowerInvariant() },
            });
        }

        BulkAsync(lines).GetAwaiter().GetResult();
    }

    public PagedResult<CatalogSearchResultEntry> Search(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        var languages = language is null ? EsCatalogFields.Languages : [language];
        var from = (page - 1) * pageSize;
        var size = pageSize == int.MaxValue ? 10000 : pageSize;

        var body = CatalogQueryBuilder.BuildSearchBody(
            queryString, languages, searchFilter,
            _userContextService.GetUserBusinessRole(), _userContextService.GetUserAgencies(), from, size);

        var response = RequestAsync(Elastic.Transport.HttpMethod.POST, $"/{_index}/_search", Serialize(body), CancellationToken.None)
            .GetAwaiter().GetResult();

        return ParseSearchResponse(ReadBodyOrLog(response, "catalog search"), page, pageSize);
    }

    public IEnumerable<CatalogSearchCountResultEntry> SearchCount(string? queryString, string? language, CatalogSearchFilter? searchFilter)
    {
        var languages = language is null ? EsCatalogFields.Languages : [language];

        var body = CatalogQueryBuilder.BuildCountBody(
            queryString, languages, searchFilter,
            _userContextService.GetUserBusinessRole(), _userContextService.GetUserAgencies());

        var response = RequestAsync(Elastic.Transport.HttpMethod.POST, $"/{_index}/_search", Serialize(body), CancellationToken.None)
            .GetAwaiter().GetResult();

        return ParseCountResponse(ReadBodyOrLog(response, "catalog search count"));
    }

    private void BulkIndex(IEnumerable<(string Id, Dictionary<string, object?> Document)> documents)
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

        if (lines.Count == 0)
        {
            return;
        }

        BulkAsync(lines).GetAwaiter().GetResult();
    }

    private async Task BulkAsync(List<object> ndjsonLines)
    {
        var response = await _client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            "/_bulk",
            PostData.MultiJson(ndjsonLines));

        if (response.ApiCallDetails?.HasSuccessfulStatusCode != true || EsRest.HasBulkErrors(response.Body))
        {
            _logger.LogError("Elasticsearch bulk request failed or reported item errors: {Body}", response.Body);
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
    private string ReadBodyOrLog(StringResponse response, string operation)
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
        PublisherIdentifier = GetString(src, EsCatalogFields.PublisherIdentifier),
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

        var total = root.TryGetProperty("hits", out var hitsRoot)
            && hitsRoot.TryGetProperty("total", out var totalEl)
            && totalEl.TryGetProperty("value", out var totalValue)
            ? totalValue.GetInt32()
            : 0;

        var entries = new List<CatalogSearchCountResultEntry>();
        if (!root.TryGetProperty("aggregations", out var aggs))
        {
            return entries;
        }

        foreach (var agg in aggs.EnumerateObject())
        {
            if (!agg.Value.TryGetProperty("buckets", out var buckets))
            {
                continue;
            }

            var counts = new Dictionary<string, int>();
            foreach (var bucket in buckets.EnumerateArray())
            {
                var key = bucket.GetProperty("key");
                var label = key.ValueKind == JsonValueKind.Number ? key.GetInt64().ToString() : key.GetString() ?? string.Empty;
                counts[label] = bucket.GetProperty("doc_count").GetInt32();
            }

            entries.Add(new CatalogSearchCountResultEntry
            {
                Identifier = agg.Name,
                TotalDocumentsCount = total,
                CountByValues = counts.AsReadOnly(),
            });
        }

        return entries;
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
