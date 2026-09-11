using System.Text.Json;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal static class CodeListIndexMapping
{
    public static string BuildCreateIndexJson() => JsonSerializer.Serialize(new Dictionary<string, object?>
    {
        ["settings"] = new Dictionary<string, object?>
        {
            ["analysis"] = new Dictionary<string, object?>
            {
                ["tokenizer"] = EsAnalysis.BuildTokenizers(),
                ["filter"] = EsAnalysis.BuildFilters(),
                ["analyzer"] = EsAnalysis.BuildAnalyzers(),
                ["normalizer"] = EsAnalysis.BuildNormalizers(),
            },
        },
        ["mappings"] = new Dictionary<string, object?> { ["properties"] = BuildProperties() },
    });

    private static Dictionary<string, object?> BuildProperties()
    {
        var properties = new Dictionary<string, object?>
        {
            [EsCodeListFields.Id] = EsAnalysis.Keyword(),
            [EsCodeListFields.ConceptId] = EsAnalysis.Keyword(),
            [EsCodeListFields.ParentCode] = EsAnalysis.Keyword(),
            [EsCodeListFields.AncestorCodes] = EsAnalysis.Keyword(),
            [EsCodeListFields.Annotations] = new Dictionary<string, object?>
            {
                ["type"] = "nested",
                ["properties"] = BuildAnnotationProperties(),
            },
        };

        foreach (var field in EsCodeListFields.SearchableKeywordFields)
        {
            properties[field] = EsAnalysis.SearchableKeywordCaseInsensitive();
        }

        foreach (var field in EsCodeListFields.NgramFields)
        {
            properties[field] = new Dictionary<string, object?>
            {
                ["properties"] = EsAnalysis.MultiLanguageProperties(EsCodeListFields.Languages),
            };
        }

        foreach (var field in EsCodeListFields.PlainMultiLanguageFields)
        {
            properties[field] = new Dictionary<string, object?>
            {
                ["properties"] = EsAnalysis.PlainMultiLanguageProperties(EsCodeListFields.Languages),
            };
        }

        return properties;
    }

    private static Dictionary<string, object?> BuildAnnotationProperties()
    {
        var properties = new Dictionary<string, object?>
        {
            [EsCodeListFields.Annotation.Text] = new Dictionary<string, object?>
            {
                ["properties"] = EsAnalysis.PlainMultiLanguageProperties(
                    EsCodeListFields.Languages,
                    withExact: true),
            },
        };

        foreach (var field in EsCodeListFields.Annotation.SearchableKeywordFields)
        {
            properties[field] = EsAnalysis.SearchableKeywordCaseInsensitive();
        }

        return properties;
    }
}
