using System.Text.Json;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogIndexMapping
{
    public static string BuildCreateIndexJson() => JsonSerializer.Serialize(new Dictionary<string, object?>
    {
        ["settings"] = new Dictionary<string, object?>
        {
            ["analysis"] = new Dictionary<string, object?>
            {
                ["filter"] = EsAnalysis.BuildFilters(),
                ["analyzer"] = EsAnalysis.BuildAnalyzers(),
            },
        },
        ["mappings"] = new Dictionary<string, object?> { ["properties"] = BuildProperties() },
    });

    private static Dictionary<string, object?> BuildProperties()
    {
        var properties = new Dictionary<string, object?>
        {
            [EsCatalogFields.Id] = EsAnalysis.Keyword(),
            [EsCatalogFields.Identifier] = EsAnalysis.Keyword(),
            [EsCatalogFields.DataOwner] = EsAnalysis.Keyword(),
            [EsCatalogFields.Version] = EsAnalysis.Keyword(),
            [EsCatalogFields.Publisher] = EsAnalysis.Keyword(),
            [EsCatalogFields.PublisherIdentifier] = EsAnalysis.Keyword(),
            [EsCatalogFields.PublisherIdentifierLabel] = EsAnalysis.Keyword(),
            [EsCatalogFields.RegistrationStatus] = EsAnalysis.Keyword(),
            [EsCatalogFields.RegistrationStatusProposal] = EsAnalysis.Keyword(),
            [EsCatalogFields.RegistrationStatusWeight] = new Dictionary<string, object?> { ["type"] = "integer" },
            [EsCatalogFields.PublicationLevel] = EsAnalysis.Keyword(),
            [EsCatalogFields.PublicationLevelProposal] = EsAnalysis.Keyword(),
            [EsCatalogFields.ConceptType] = EsAnalysis.Keyword(),
            [EsCatalogFields.Type] = EsAnalysis.Keyword(),
            [EsCatalogFields.Themes] = EsAnalysis.Keyword(),
            [EsCatalogFields.AccessRights] = EsAnalysis.Keyword(),
            [EsCatalogFields.BusinessEvents] = EsAnalysis.Keyword(),
            [EsCatalogFields.LifeEvents] = EsAnalysis.Keyword(),
            [EsCatalogFields.Formats] = EsAnalysis.Keyword(),
            [EsCatalogFields.HasStructure] = new Dictionary<string, object?> { ["type"] = "boolean" },
            [EsCatalogFields.ResponsiblePersonName] = EsAnalysis.Keyword(),
            [EsCatalogFields.ResponsibleDeputyName] = EsAnalysis.Keyword(),
            [EsCatalogFields.CreatedAt] = new Dictionary<string, object?> { ["type"] = "date" },
            [EsCatalogFields.ModifiedAt] = new Dictionary<string, object?> { ["type"] = "date" },
            [EsCatalogFields.ValidFrom] = new Dictionary<string, object?> { ["type"] = "date" },
            [EsCatalogFields.ValidTo] = new Dictionary<string, object?> { ["type"] = "date" },
            [EsCatalogFields.CreationType] = EsAnalysis.Keyword(),
        };

        foreach (var field in EsCatalogFields.MultiLanguageFields)
        {
            properties[field] = new Dictionary<string, object?>
            {
                ["properties"] = EsAnalysis.MultiLanguageProperties(EsCatalogFields.Languages),
            };
        }

        foreach (var field in EsCatalogFields.EmailFields)
        {
            properties[field] = EsAnalysis.Keyword();
        }

        return properties;
    }
}
