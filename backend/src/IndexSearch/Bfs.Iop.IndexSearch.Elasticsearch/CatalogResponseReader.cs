using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogResponseReader
{
    public static PagedResult<CatalogSearchHit> ReadSearch(JsonElement response, int page, int pageSize)
    {
        var hits = response.GetProperty("hits");

        return new PagedResult<CatalogSearchHit>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = hits.GetProperty("total").GetProperty("value").GetInt32(),
            Results = [.. hits.GetProperty("hits").EnumerateArray().Select(x => ReadHit(x.GetProperty("_source")))],
        };
    }

    public static CatalogFacetCounts ReadFacets(JsonElement response)
    {
        if (!response.TryGetProperty("aggregations", out var aggregations))
        {
            return new CatalogFacetCounts();
        }

        return new CatalogFacetCounts
        {
            // Not hits.total: that ignores the facet selections, while the Total aggregation applies
            // them. The two disagree the moment a filter is set.
            TotalCount = aggregations.TryGetProperty(CatalogFacetDimensions.Total, out var total)
                ? total.GetProperty("doc_count").GetInt32()
                : 0,

            Publishers = Buckets(aggregations, CatalogFacetDimensions.PublisherIdentifier),
            Types = Buckets(aggregations, CatalogFacetDimensions.Type),
            Themes = Buckets(aggregations, CatalogFacetDimensions.Themes),
            AccessRights = Buckets(aggregations, CatalogFacetDimensions.AccessRights),
            Formats = Buckets(aggregations, CatalogFacetDimensions.Formats),
            BusinessEvents = Buckets(aggregations, CatalogFacetDimensions.BusinessEvents),
            LifeEvents = Buckets(aggregations, CatalogFacetDimensions.LifeEvents),
            ConceptTypes = Buckets(aggregations, CatalogFacetDimensions.ConceptType),
            PublicationLevels = Buckets(aggregations, CatalogFacetDimensions.PublicationLevel),
            PublicationLevelProposals = Buckets(aggregations, CatalogFacetDimensions.PublicationLevelProposal),
            RegistrationStatuses = Buckets(aggregations, CatalogFacetDimensions.RegistrationStatus),
            RegistrationStatusProposals = Buckets(aggregations, CatalogFacetDimensions.RegistrationStatusProposal),
            Structures = Buckets(aggregations, CatalogFacetDimensions.HasStructure),
        };
    }

    private static CatalogSearchHit ReadHit(JsonElement source) => new()
    {
        Id = Guid.Parse(String(source, EsCatalogFields.Id)!),
        Type = Enum<SearchResourceType>(source, EsCatalogFields.Type) ?? SearchResourceType.Dataset,
        Identifiers = Strings(source, EsCatalogFields.Identifier),
        PublisherId = Guid.TryParse(String(source, EsCatalogFields.Publisher), out var publisher)
            ? publisher
            : Guid.Empty,
        PublicationLevel = Enum<PublicationLevel>(source, EsCatalogFields.PublicationLevel)
            ?? PublicationLevel.Internal,
        PublicationLevelProposal = Enum<PublicationLevel>(source, EsCatalogFields.PublicationLevelProposal),
        RegistrationStatus = Enum<RegistrationStatus>(source, EsCatalogFields.RegistrationStatus)
            ?? RegistrationStatus.Incomplete,
        RegistrationStatusProposal = Enum<RegistrationStatus>(source, EsCatalogFields.RegistrationStatusProposal),
        CreatedAt = Date(source, EsCatalogFields.CreatedAt),
        ModifiedAt = Date(source, EsCatalogFields.ModifiedAt),
        CreationType = Enum<CreationType>(source, EsCatalogFields.CreationType) ?? CreationType.Manual,
        Title = MultiLang(source, EsCatalogFields.Title),
        Name = MultiLang(source, EsCatalogFields.Name),
        Description = MultiLang(source, EsCatalogFields.Description),
        Version = String(source, EsCatalogFields.Version),
        Themes = Strings(source, EsCatalogFields.Themes),
        AccessRights = String(source, EsCatalogFields.AccessRights),
        Formats = Strings(source, EsCatalogFields.Formats),
        BusinessEvents = Strings(source, EsCatalogFields.BusinessEvents),
        LifeEvents = Strings(source, EsCatalogFields.LifeEvents),
        ConceptType = Enum<ConceptType>(source, EsCatalogFields.ConceptType),
        ValidFrom = Date(source, EsCatalogFields.ValidFrom),
        ValidTo = Date(source, EsCatalogFields.ValidTo),
        HasStructure = source.TryGetProperty(EsCatalogFields.HasStructure, out var structure)
            ? structure.GetBoolean()
            : null,
    };

    private static IReadOnlyDictionary<string, int> Buckets(JsonElement aggregations, string dimension)
    {
        if (!aggregations.TryGetProperty(dimension, out var dimensionAggregation)
            || !dimensionAggregation.TryGetProperty("values", out var values))
        {
            return new Dictionary<string, int>();
        }

        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var bucket in values.GetProperty("buckets").EnumerateArray())
        {
            var key = bucket.GetProperty("key");

            // A terms aggregation on a boolean field keys its buckets 0 and 1, not "false" and "true".
            var name = key.ValueKind switch
            {
                JsonValueKind.String => key.GetString()!,
                JsonValueKind.Number => (key.GetInt64() != 0).ToString(),
                JsonValueKind.True or JsonValueKind.False => key.GetBoolean().ToString(),
                _ => key.ToString(),
            };

            counts[name] = bucket.GetProperty("doc_count").GetInt32();
        }

        return counts;
    }

    private static string? String(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // The factory omits an empty array entirely, and writes a single value as a bare string when the
    // document held one. Both have to read back as a list.
    private static IReadOnlyList<string> Strings(JsonElement source, string field)
    {
        if (!source.TryGetProperty(field, out var value))
        {
            return [];
        }

        return value.ValueKind switch
        {
            JsonValueKind.Array => [.. value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString()!)],
            JsonValueKind.String => [value.GetString()!],
            _ => [],
        };
    }

    private static TEnum? Enum<TEnum>(JsonElement source, string field) where TEnum : struct, System.Enum =>
        String(source, field) is { } text && System.Enum.TryParse<TEnum>(text, out var parsed)
            ? parsed
            : null;

    private static DateTimeOffset? Date(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.TryGetDateTimeOffset(out var date)
            ? date
            : null;

    private static MultiLanguageModel? MultiLang(JsonElement source, string field)
    {
        if (!source.TryGetProperty(field, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var text = new MultiLanguageModel
        {
            De = Language(value, MultiLanguageModel.GermanKey),
            En = Language(value, MultiLanguageModel.EnglishKey),
            Fr = Language(value, MultiLanguageModel.FrenchKey),
            It = Language(value, MultiLanguageModel.ItalianKey),
            Rm = Language(value, MultiLanguageModel.RomanshKey),
        };

        return text.IsContentNullOrWhiteSpace() ? null : text;
    }

    // Keywords are stored per language as arrays; a hit shows the first, which is what the old index
    // returned too.
    private static string? Language(JsonElement multiLanguage, string language)
    {
        if (!multiLanguage.TryGetProperty(language, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Array => value.EnumerateArray()
                .FirstOrDefault(x => x.ValueKind == JsonValueKind.String).GetString(),
            _ => null,
        };
    }
}
