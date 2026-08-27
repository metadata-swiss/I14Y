using System.Text.Json;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Builds the create-index request body (analysis settings + field mappings) for the catalog index.
/// Per-language analyzers (stopwords, stemming, ASCII-folding) and an ngram(2–3) sub-field per
/// multilingual field for partial matching. Analysis is fixed when the index is created, so a change
/// here does nothing until the index is rebuilt.
/// </summary>
internal static class CatalogIndexMapping
{
    public static string BuildCreateIndexJson()
    {
        var body = new Dictionary<string, object?>
        {
            ["settings"] = new Dictionary<string, object?>
            {
                ["analysis"] = new Dictionary<string, object?>
                {
                    ["filter"] = EsAnalysis.BuildFilters(),
                    ["analyzer"] = EsAnalysis.BuildAnalyzers(),
                },
            },
            ["mappings"] = new Dictionary<string, object?>
            {
                ["properties"] = BuildProperties(),
            },
        };

        return JsonSerializer.Serialize(body);
    }

    private static Dictionary<string, object?> BuildProperties()
    {
        var props = new Dictionary<string, object?>();

        foreach (var field in EsCatalogFields.MultiLanguageFields)
        {
            props[field] = new Dictionary<string, object?> { ["properties"] = EsAnalysis.MultiLanguageProperties(EsCatalogFields.Languages) };
        }

        // Identifier: analyzed (lowercased) for matching, with an exact keyword sub-field.
        props[EsCatalogFields.Identifier] = new Dictionary<string, object?>
        {
            ["type"] = "text",
            ["analyzer"] = "i14y_ngram_search",
            ["fields"] = new Dictionary<string, object?> { ["keyword"] = new Dictionary<string, object?> { ["type"] = "keyword" } },
        };

        props[EsCatalogFields.DataOwner] = Text("i14y_ngram_search");
        props[EsCatalogFields.ResponsiblePersonName] = Text("i14y_ngram_search");
        props[EsCatalogFields.ResponsibleDeputyName] = Text("i14y_ngram_search");

        // Exact keyword fields.
        foreach (var f in new[]
                 {
                     EsCatalogFields.Id, EsCatalogFields.Version, EsCatalogFields.Publisher,
                     EsCatalogFields.PublisherIdentifier, EsCatalogFields.PublisherIdentifierLabel,
                     EsCatalogFields.Type, EsCatalogFields.Themes,
                     EsCatalogFields.AccessRights, EsCatalogFields.BusinessEvents, EsCatalogFields.LifeEvents,
                     EsCatalogFields.Formats, EsCatalogFields.ContactPointHasEmail,
                     EsCatalogFields.ResponsiblePersonEmail, EsCatalogFields.ResponsibleDeputyEmail,
                 })
        {
            props[f] = new Dictionary<string, object?> { ["type"] = "keyword" };
        }

        // Numeric fields.
        foreach (var f in new[]
                 {
                     EsCatalogFields.RegistrationStatus, EsCatalogFields.RegistrationStatusProposal,
                     EsCatalogFields.RegistrationStatusWeight, EsCatalogFields.PublicationLevel,
                     EsCatalogFields.PublicationLevelProposal, EsCatalogFields.ConceptType, EsCatalogFields.CreationType,
                 })
        {
            props[f] = new Dictionary<string, object?> { ["type"] = "integer" };
        }

        props[EsCatalogFields.HasStructure] = new Dictionary<string, object?> { ["type"] = "boolean" };

        foreach (var f in new[] { EsCatalogFields.CreatedAt, EsCatalogFields.ModifiedAt, EsCatalogFields.ValidFrom, EsCatalogFields.ValidTo })
        {
            props[f] = new Dictionary<string, object?> { ["type"] = "date" };
        }

        return props;
    }

    private static Dictionary<string, object?> Text(string analyzer) => new()
    {
        ["type"] = "text",
        ["analyzer"] = analyzer,
    };
}
