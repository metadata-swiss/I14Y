using System.Text.Json;

namespace Bfs.Iop.Search.Elasticsearch.CodeList;

/// <summary>
/// Create-index request body for the codelist index: shared per-language analysis, plus a
/// <c>nested</c> annotations mapping. Text fields carry a <c>.raw</c> keyword sub-field so the
/// annotation filters can match exact (lowercased) values.
/// </summary>
internal static class CodeListIndexMapping
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

    private static Dictionary<string, object?> BuildProperties() => new()
    {
        [EsCodeListFields.Id] = Keyword(),
        [EsCodeListFields.ConceptId] = Keyword(),
        [EsCodeListFields.ParentCode] = Keyword(),
        // Code: analyzed+lowercased for prefix matching, plus an exact keyword sub-field.
        [EsCodeListFields.Code] = new Dictionary<string, object?>
        {
            ["type"] = "text",
            ["analyzer"] = "i14y_ngram_search",
            ["fields"] = new Dictionary<string, object?> { ["keyword"] = Keyword() },
        },
        [EsCodeListFields.Name] = new Dictionary<string, object?> { ["properties"] = EsAnalysis.MultiLanguageProperties(EsCodeListFields.Languages) },
        [EsCodeListFields.Description] = new Dictionary<string, object?> { ["properties"] = EsAnalysis.MultiLanguageProperties(EsCodeListFields.Languages) },
        [EsCodeListFields.Annotations] = new Dictionary<string, object?>
        {
            ["type"] = "nested",
            ["properties"] = new Dictionary<string, object?>
            {
                [EsCodeListFields.AnnotationType] = TextWithRaw("i14y_ngram_search"),
                [EsCodeListFields.AnnotationIdentifier] = TextWithRaw("i14y_ngram_search"),
                [EsCodeListFields.AnnotationTitle] = TextWithRaw("i14y_ngram_search"),
                [EsCodeListFields.AnnotationUri] = TextWithRaw("i14y_ngram_search"),
                [EsCodeListFields.AnnotationText] = new Dictionary<string, object?> { ["properties"] = AnnotationTextProperties() },
            },
        },
    };

    private static Dictionary<string, object?> AnnotationTextProperties()
    {
        var langs = new Dictionary<string, object?>();
        foreach (var lang in EsCodeListFields.Languages)
        {
            langs[lang] = TextWithRaw($"i14y_{lang}");
        }

        return langs;
    }

    private static Dictionary<string, object?> Keyword() => new() { ["type"] = "keyword" };

    private static Dictionary<string, object?> TextWithRaw(string analyzer) => new()
    {
        ["type"] = "text",
        ["analyzer"] = analyzer,
        ["fields"] = new Dictionary<string, object?> { ["raw"] = Keyword() },
    };
}
