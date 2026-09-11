using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// An ngram token filter emits every gram of a word at one position, and Lucene collapses terms that
// share a position into a single synonym clause. minimum_should_match then asks for one clause, so
// sharing a single gram with a field was enough to match it: against the live catalogue "Wetterdaten"
// returned 2389 of 2935 documents. The grams have to come from the tokenizer, which gives each its own
// position and puts minimum_should_match back in charge.
[TestFixture]
internal sealed class NgramAnalysisTests
{
    [TestCase(Catalog)]
    [TestCase(CodeList)]
    public void The_grams_come_from_the_tokenizer(string index)
    {
        var analysis = Analysis(index);

        analysis.GetProperty("analyzer").GetProperty("i14y_ngram")
            .GetProperty("tokenizer").GetString().Should().Be(EsAnalysis.NgramTokenizer);

        var tokenizer = analysis.GetProperty("tokenizer").GetProperty(EsAnalysis.NgramTokenizer);

        tokenizer.GetProperty("type").GetString().Should().Be("ngram");
        tokenizer.GetProperty("min_gram").GetInt32().Should().Be(2);
        tokenizer.GetProperty("max_gram").GetInt32().Should().Be(3);

        // Without them a gram spans the space between two words, and "der Bund" would answer to "rb".
        tokenizer.GetProperty("token_chars").EnumerateArray().Select(x => x.GetString())
            .Should().BeEquivalentTo(["letter", "digit"]);
    }

    [TestCase(Catalog)]
    [TestCase(CodeList)]
    public void No_token_filter_shreds_a_word_into_grams(string index)
    {
        foreach (var filter in Analysis(index).GetProperty("filter").EnumerateObject())
        {
            filter.Value.GetProperty("type").GetString().Should()
                .NotBe("ngram", "'{0}' would stack every gram on one position", filter.Name);
        }
    }

    private const string Catalog = "catalog";
    private const string CodeList = "codelist";

    private static JsonElement Analysis(string index)
    {
        var json = index == Catalog
            ? CatalogIndexMapping.BuildCreateIndexJson()
            : CodeListIndexMapping.BuildCreateIndexJson();

        using var document = JsonDocument.Parse(json);

        return document.RootElement.GetProperty("settings").GetProperty("analysis").Clone();
    }
}
