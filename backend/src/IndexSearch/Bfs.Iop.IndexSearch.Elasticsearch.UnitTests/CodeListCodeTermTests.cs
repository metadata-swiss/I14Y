using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// CHOP "41.42" on DEV ranked 41.0C.2, 41.0C.42, 41.0B.2 and 41.0A.2 above the sub-codes 41.42.00 to
// 41.42.99, and listed those sub-codes in no order: every prefix hit scores the same flat boost, while
// fuzzy and ngram text matches on unrelated entries added up to more.
[TestFixture]
internal sealed class CodeListCodeTermTests
{
    private static readonly Guid Concept = Guid.Parse("08dadd0b-a8bb-0855-8607-f4a10a21126f");

    [Test]
    public void An_exact_code_scores_above_the_codes_it_prefixes()
    {
        var scoring = DefaultScoring.CodeList;

        using var body = Body("41.42", scoring);

        var exact = Clauses(body.RootElement, "term")
            .Where(x => x.TryGetProperty(EsCodeListFields.Code, out _))
            .Select(x => x.GetProperty(EsCodeListFields.Code))
            .Single();

        exact.GetProperty("value").GetString().Should().Be("41.42");
        exact.GetProperty("boost").GetDouble().Should().Be(scoring.ExactCodeBoost);
        Clauses(body.RootElement, "prefix")
            .Count(x => x.TryGetProperty(EsCodeListFields.Code, out _))
            .Should().Be(1);
    }

    [TestCase("1")]
    [TestCase("1a")]
    public void A_short_code_also_gets_the_exact_code_boost(string query)
    {
        var scoring = DefaultScoring.CodeList;

        using var body = Body(query, scoring);

        Clauses(body.RootElement, "term")
            .Where(x => x.TryGetProperty(EsCodeListFields.Code, out _))
            .Select(x => x.GetProperty(EsCodeListFields.Code))
            .Should().Contain(x => x.GetProperty("boost").GetDouble() == scoring.ExactCodeBoost);
    }

    [TestCase("41.42")]
    [TestCase("41.0C.2")]
    [TestCase("A-12")]
    public void A_code_like_term_is_matched_without_typos_or_ngrams(string query)
    {
        var json = Json(query);

        json.Should().NotContain("fuzziness").And.NotContain(".ngram");
    }

    [Test]
    public void A_word_keeps_its_typo_tolerance_and_ngrams()
    {
        Json("zürch").Should().Contain("fuzziness").And.Contain(".ngram");
    }

    [Test]
    public void Equal_scores_are_ordered_by_code()
    {
        using var body = Body("41.42", DefaultScoring.CodeList);

        body.RootElement.GetProperty("sort").EnumerateArray()
            .Select(x => x.EnumerateObject().Single().Name)
            .Should().Equal("_score", EsCodeListFields.Code);
    }

    [Test]
    public void An_unpaged_search_orders_ties_by_code_like_a_paged_one()
    {
        var body = CodeListQueryBuilder.BuildPointInTimeBody(
            Concept, "41.42", "de", filter: null, DefaultScoring.CodeList, "pit", TimeSpan.FromMinutes(1), size: 10, searchAfter: null);

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(body));

        json.RootElement.GetProperty("sort").EnumerateArray()
            .Select(x => x.EnumerateObject().Single().Name)
            .Should().Equal("_score", EsCodeListFields.Code, "_shard_doc");
    }

    private static string Json(string query) =>
        JsonSerializer.Serialize(Build(query, DefaultScoring.CodeList));

    private static JsonDocument Body(string query, CodeListScoring scoring) =>
        JsonDocument.Parse(JsonSerializer.Serialize(Build(query, scoring)));

    private static Dictionary<string, object?> Build(string query, CodeListScoring scoring) =>
        CodeListQueryBuilder.BuildSearchBody(Concept, query, "de", filter: null, scoring, from: 0, size: 10);

    private static IEnumerable<JsonElement> Clauses(JsonElement element, string kind)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == kind)
                {
                    yield return property.Value;
                }

                foreach (var nested in Clauses(property.Value, kind))
                {
                    yield return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var nested in element.EnumerateArray().SelectMany(x => Clauses(x, kind)))
            {
                yield return nested;
            }
        }
    }
}
