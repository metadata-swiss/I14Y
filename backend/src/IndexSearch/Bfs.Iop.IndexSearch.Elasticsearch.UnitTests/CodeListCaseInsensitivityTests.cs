using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeList;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// A third of the code list carries an uppercase letter in its code — E34.51, G83.2, 39.BH.75. The
// free text query lowercases every term and matches code as a raw keyword, so without a normalizer on
// the mapping side a term query for "g83.2" cannot reach a document indexed as "G83.2", and 156k
// entries become unfindable by their own code.
[TestFixture]
internal sealed class CodeListCaseInsensitivityTests
{
    [Test]
    public void Code_is_matched_whatever_its_case()
    {
        Properties().GetProperty(EsCodeListFields.Code)
            .GetProperty("normalizer").GetString()
            .Should().Be(EsAnalysis.LowercaseNormalizer);
    }

    [TestCase(EsCodeListFields.Annotation.Identifier)]
    [TestCase(EsCodeListFields.Annotation.Uri)]
    public void An_annotation_keyword_the_query_matches_raw_is_matched_whatever_its_case(string field)
    {
        Properties().GetProperty(EsCodeListFields.Annotations)
            .GetProperty("properties").GetProperty(field)
            .GetProperty("normalizer").GetString()
            .Should().Be(EsAnalysis.LowercaseNormalizer);
    }

    [Test]
    public void Code_keeps_the_analysed_copy_free_text_scores_against()
    {
        var code = Properties().GetProperty(EsCodeListFields.Code);

        code.GetProperty("type").GetString().Should().Be("keyword");
        code.GetProperty("fields").GetProperty(EsAnalysis.TextSubField)
            .GetProperty("type").GetString().Should().Be("text");
    }

    private static JsonElement Properties()
    {
        var document = JsonDocument.Parse(CodeListIndexMapping.BuildCreateIndexJson());

        return document.RootElement.GetProperty("mappings").GetProperty("properties").Clone();
    }
}
