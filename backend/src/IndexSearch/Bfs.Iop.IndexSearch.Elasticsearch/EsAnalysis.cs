namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class EsAnalysis
{
    public const string TextSubField = "text";

    public const string RawSubField = "raw";

    public const string LowercaseNormalizer = "i14y_lowercase";

    public const string NgramTokenizer = "i14y_ngram_tokenizer";

    // The grams have to come from the tokenizer and not from a filter of the same name. A filter emits
    // every gram of a word at one position, which Lucene collapses into a single synonym clause, and a
    // minimum_should_match over one clause asks for one gram: sharing "er" with a title was enough to
    // match it. Measured against the live catalogue, "Wetterdaten" pulled 2389 of 2935 documents.
    public static Dictionary<string, object?> BuildTokenizers() => new()
    {
        [NgramTokenizer] = new Dictionary<string, object?>
        {
            ["type"] = "ngram",
            ["min_gram"] = 2,
            ["max_gram"] = 3,
            ["token_chars"] = new[] { "letter", "digit" },
        },
    };

    public static Dictionary<string, object?> BuildFilters() => new()
    {
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
        ["i14y_ngram"] = AnalyzerOn(NgramTokenizer, "lowercase", "asciifolding"),
        ["i14y_text"] = Analyzer("lowercase", "asciifolding"),
    };

    public static Dictionary<string, object?> Analyzer(params string[] filters) =>
        AnalyzerOn("standard", filters);

    public static Dictionary<string, object?> AnalyzerOn(string tokenizer, params string[] filters) => new()
    {
        ["type"] = "custom",
        ["tokenizer"] = tokenizer,
        ["filter"] = filters,
    };

    public static Dictionary<string, object?> Keyword() => new() { ["type"] = "keyword" };

    public static Dictionary<string, object?> SearchableKeyword() => new()
    {
        ["type"] = "keyword",
        ["fields"] = new Dictionary<string, object?>
        {
            [TextSubField] = new Dictionary<string, object?>
            {
                ["type"] = "text",
                ["analyzer"] = "i14y_text",
            },
        },
    };

    // Lets a term query match a keyword whatever its case, without lowercasing the value we send. The
    // document _source keeps the original, so a search hit still renders "Erste" and not "erste".
    public static Dictionary<string, object?> BuildNormalizers() => new()
    {
        [LowercaseNormalizer] = new Dictionary<string, object?>
        {
            ["type"] = "custom",
            ["filter"] = new[] { "lowercase", "asciifolding" },
        },
    };

    // A keyword filtered case-insensitively, plus the analysed copy free text matches.
    public static Dictionary<string, object?> SearchableKeywordCaseInsensitive() => new()
    {
        ["type"] = "keyword",
        ["normalizer"] = LowercaseNormalizer,
        ["fields"] = new Dictionary<string, object?>
        {
            [TextSubField] = new Dictionary<string, object?>
            {
                ["type"] = "text",
                ["analyzer"] = "i14y_text",
            },
        },
    };

    // Analysed per language, with no ngram copy. For fields free text matches whole words in and a
    // filter matches exactly — the raw sub-field is what a term query targets.
    public static Dictionary<string, object?> PlainMultiLanguageProperties(
        IEnumerable<string> languages,
        bool withExact = false)
    {
        var langs = new Dictionary<string, object?>();

        foreach (var lang in languages)
        {
            var property = new Dictionary<string, object?>
            {
                ["type"] = "text",
                ["analyzer"] = $"i14y_{lang}",
            };

            if (withExact)
            {
                property["fields"] = new Dictionary<string, object?> { [RawSubField] = Keyword() };
            }

            langs[lang] = property;
        }

        return langs;
    }

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
                    },
                },
            };
        }

        return langs;
    }
}
