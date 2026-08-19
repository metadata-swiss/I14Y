namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Shared Elasticsearch analysis settings (token filters + analyzers) used by both the catalog and the
/// codelist index mappings. Elasticsearch equivalent of the Lucene <c>LanguageDependentAnalyzer</c>:
/// per-language stopwords/stemming/ASCII-folding, plus an ngram(2–3) analyzer for partial matching.
/// </summary>
internal static class EsAnalysis
{
    public static Dictionary<string, object?> BuildFilters() => new()
    {
        ["ngram_2_3"] = new Dictionary<string, object?> { ["type"] = "ngram", ["min_gram"] = 2, ["max_gram"] = 3 },
        ["german_stop"] = new Dictionary<string, object?> { ["type"] = "stop", ["stopwords"] = "_german_" },
        ["german_stemmer"] = new Dictionary<string, object?> { ["type"] = "stemmer", ["language"] = "light_german" },
        ["english_stop"] = new Dictionary<string, object?> { ["type"] = "stop", ["stopwords"] = "_english_" },
        ["english_stemmer"] = new Dictionary<string, object?> { ["type"] = "stemmer", ["language"] = "english" },
        ["french_elision"] = new Dictionary<string, object?>
        {
            ["type"] = "elision",
            ["articles_case"] = true,
            ["articles"] = new[] { "l", "m", "t", "qu", "n", "s", "j", "d", "c", "jusqu", "quoiqu", "lorsqu", "puisqu" },
        },
        ["french_stop"] = new Dictionary<string, object?> { ["type"] = "stop", ["stopwords"] = "_french_" },
        ["french_stemmer"] = new Dictionary<string, object?> { ["type"] = "stemmer", ["language"] = "light_french" },
        ["italian_stop"] = new Dictionary<string, object?> { ["type"] = "stop", ["stopwords"] = "_italian_" },
        ["italian_stemmer"] = new Dictionary<string, object?> { ["type"] = "stemmer", ["language"] = "light_italian" },
    };

    public static Dictionary<string, object?> BuildAnalyzers() => new()
    {
        ["i14y_de"] = Analyzer("lowercase", "german_stop", "german_normalization", "german_stemmer", "asciifolding"),
        ["i14y_en"] = Analyzer("lowercase", "english_stop", "english_stemmer", "asciifolding"),
        ["i14y_fr"] = Analyzer("french_elision", "lowercase", "french_stop", "french_stemmer", "asciifolding"),
        ["i14y_it"] = Analyzer("lowercase", "italian_stop", "italian_stemmer", "asciifolding"),
        ["i14y_rm"] = Analyzer("lowercase", "asciifolding"),
        // Index-time ngram; searched with a non-ngram analyzer so the query term isn't itself split into grams.
        ["i14y_ngram"] = Analyzer("lowercase", "asciifolding", "ngram_2_3"),
        ["i14y_ngram_search"] = Analyzer("lowercase", "asciifolding"),
    };

    public static Dictionary<string, object?> Analyzer(params string[] filters) => new()
    {
        ["type"] = "custom",
        ["tokenizer"] = "standard",
        ["filter"] = filters,
    };

    /// <summary>A text field with per-language analyzer and a `.ngram` sub-field for partial matching.</summary>
    public static Dictionary<string, object?> MultiLanguageProperties(IEnumerable<string> languages)
    {
        var langs = new Dictionary<string, object?>();
        foreach (var lang in languages)
        {
            langs[lang] = new Dictionary<string, object?>
            {
                ["type"] = "text",
                ["analyzer"] = $"i14y_{lang}",
                ["fields"] = new Dictionary<string, object?>
                {
                    ["ngram"] = new Dictionary<string, object?>
                    {
                        ["type"] = "text",
                        ["analyzer"] = "i14y_ngram",
                        ["search_analyzer"] = "i14y_ngram_search",
                    },
                },
            };
        }

        return langs;
    }
}
