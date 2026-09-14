using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogResponseReader
{
    public static PagedResult<CatalogSearchHit> ReadSearch(
        JsonElement response,
        int page,
        int pageSize,
        out int skipped)
    {
        var hits = response.GetProperty("hits");

        var read = hits.GetProperty("hits").EnumerateArray()
            .Select(x => TryReadHit(x.GetProperty("_source")))
            .ToArray();

        skipped = read.Count(x => x is null);

        return new PagedResult<CatalogSearchHit>
        {
            Page = page,
            PageSize = pageSize,

            // Elasticsearch's count of what matched. A document we could not read still matched, so
            // this may exceed Results.Count rather than being quietly adjusted down to it.
            TotalCount = hits.GetProperty("total").GetProperty("value").GetInt32(),
            Results = [.. read.OfType<CatalogSearchHit>()],
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

            Publishers = ReadBuckets(aggregations, CatalogFacetDimensions.PublisherIdentifier),
            Types = ReadBuckets(aggregations, CatalogFacetDimensions.Type),
            Themes = ReadBuckets(aggregations, CatalogFacetDimensions.Themes),
            AccessRights = ReadBuckets(aggregations, CatalogFacetDimensions.AccessRights),
            Formats = ReadBuckets(aggregations, CatalogFacetDimensions.Formats),
            BusinessEvents = ReadBuckets(aggregations, CatalogFacetDimensions.BusinessEvents),
            LifeEvents = ReadBuckets(aggregations, CatalogFacetDimensions.LifeEvents),
            ConceptTypes = ReadBuckets(aggregations, CatalogFacetDimensions.ConceptType),
            PublicationLevels = ReadBuckets(aggregations, CatalogFacetDimensions.PublicationLevel),
            PublicationLevelProposals = ReadBuckets(aggregations, CatalogFacetDimensions.PublicationLevelProposal),
            RegistrationStatuses = ReadBuckets(aggregations, CatalogFacetDimensions.RegistrationStatus),
            RegistrationStatusProposals = ReadBuckets(aggregations, CatalogFacetDimensions.RegistrationStatusProposal),
            Structures = ReadBuckets(aggregations, CatalogFacetDimensions.HasStructure),
        };
    }

    // The id is required on the hit and is what a caller opens the resource by, so a document without
    // one is unusable. Dropping that one row leaves the rest of the page answerable; throwing lost
    // all of it.
    private static CatalogSearchHit? TryReadHit(JsonElement source)
    {
        if (!Guid.TryParse(ReadString(source, EsCatalogFields.Id), out var id))
        {
            return null;
        }

        return ReadHit(source, id);
    }

    private static CatalogSearchHit ReadHit(JsonElement source, Guid id) => new()
    {
        Id = id,
        Type = ReadEnum<SearchResourceType>(source, EsCatalogFields.Type) ?? SearchResourceType.Dataset,
        Identifiers = ReadStrings(source, EsCatalogFields.Identifier),
        PublisherId = Guid.TryParse(ReadString(source, EsCatalogFields.Publisher), out var publisher)
            ? publisher
            : Guid.Empty,
        PublicationLevel = ReadEnum<PublicationLevel>(source, EsCatalogFields.PublicationLevel)
            ?? PublicationLevel.Internal,
        PublicationLevelProposal = ReadEnum<PublicationLevel>(source, EsCatalogFields.PublicationLevelProposal),
        RegistrationStatus = ReadEnum<RegistrationStatus>(source, EsCatalogFields.RegistrationStatus)
            ?? RegistrationStatus.Incomplete,
        RegistrationStatusProposal = ReadEnum<RegistrationStatus>(source, EsCatalogFields.RegistrationStatusProposal),
        CreatedAt = ReadDate(source, EsCatalogFields.CreatedAt),
        ModifiedAt = ReadDate(source, EsCatalogFields.ModifiedAt),
        CreationType = ReadEnum<CreationType>(source, EsCatalogFields.CreationType) ?? CreationType.Manual,
        Title = ReadMultiLanguage(source, EsCatalogFields.Title),
        Name = ReadMultiLanguage(source, EsCatalogFields.Name),
        Description = ReadMultiLanguage(source, EsCatalogFields.Description),
        Version = ReadString(source, EsCatalogFields.Version),
        Themes = ReadStrings(source, EsCatalogFields.Themes),
        AccessRights = ReadString(source, EsCatalogFields.AccessRights),
        Formats = ReadStrings(source, EsCatalogFields.Formats),
        BusinessEvents = ReadStrings(source, EsCatalogFields.BusinessEvents),
        LifeEvents = ReadStrings(source, EsCatalogFields.LifeEvents),
        ConceptType = ReadEnum<ConceptType>(source, EsCatalogFields.ConceptType),
        ValidFrom = ReadDate(source, EsCatalogFields.ValidFrom),
        ValidTo = ReadDate(source, EsCatalogFields.ValidTo),
        HasStructure = ReadBool(source, EsCatalogFields.HasStructure),
    };

    private static bool? ReadBool(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : null;

    private static IReadOnlyDictionary<string, int> ReadBuckets(JsonElement aggregations, string dimension)
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

    private static string? ReadString(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // The factory omits an empty array entirely, and writes a single value as a bare string when the
    // document held one. Both have to read back as a list.
    private static IReadOnlyList<string> ReadStrings(JsonElement source, string field)
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

    private static TEnum? ReadEnum<TEnum>(JsonElement source, string field) where TEnum : struct, Enum =>
        ReadString(source, field) is { } text && Enum.TryParse<TEnum>(text, out var parsed)
            ? parsed
            : null;

    private static DateTimeOffset? ReadDate(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.TryGetDateTimeOffset(out var date)
            ? date
            : null;

    private static MultiLanguageModel? ReadMultiLanguage(JsonElement source, string field)
    {
        if (!source.TryGetProperty(field, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var text = new MultiLanguageModel
        {
            De = ReadLanguage(value, MultiLanguageModel.GermanKey),
            En = ReadLanguage(value, MultiLanguageModel.EnglishKey),
            Fr = ReadLanguage(value, MultiLanguageModel.FrenchKey),
            It = ReadLanguage(value, MultiLanguageModel.ItalianKey),
            Rm = ReadLanguage(value, MultiLanguageModel.RomanshKey),
        };

        return text.IsContentNullOrWhiteSpace() ? null : text;
    }

    // Keywords are stored per language as arrays; a hit shows the first, which is what the old index
    // returned too.
    private static string? ReadLanguage(JsonElement multiLanguage, string language)
    {
        if (!multiLanguage.TryGetProperty(language, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),

            // JsonElement is a struct, so FirstOrDefault on an array holding no string hands back
            // default(JsonElement), whose ValueKind is Undefined and whose GetString() throws.
            JsonValueKind.Array => value.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.String)
                .Select(x => x.GetString())
                .FirstOrDefault(),

            _ => null,
        };
    }
}
