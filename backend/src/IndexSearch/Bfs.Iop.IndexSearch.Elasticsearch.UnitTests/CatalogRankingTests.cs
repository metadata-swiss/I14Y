using System.Globalization;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogRankingTests
{
    private static readonly string[] German = ["de"];

    private static JsonElement Body(string? queryString = null) =>
        JsonDocument.Parse(JsonSerializer.Serialize(CatalogQueryBuilder.BuildSearchBody(
            queryString, German, null, SearchCaller.Anonymous, from: 0, size: 10))).RootElement;

    [Test]
    public void Status_scales_the_score_rather_than_sorting_after_it()
    {
        // Status still belongs inside the score, not in the sort: it has to scale relevance rather
        // than override it. Only reuse is applied after scoring.
        var body = Body();

        SortKeys(body).Should().NotContain(EsCatalogFields.RegistrationStatusWeight);

        body.GetProperty("query").GetProperty("function_score")
            .GetProperty("boost_mode").GetString().Should().Be("multiply");
    }

    [Test]
    public void Reuse_breaks_ties_after_relevance_rather_than_competing_with_it()
    {
        // Score first, so a more reused resource can never overtake one the query matched better;
        // reuse second, so it only separates results that scored identically.
        var body = Body();

        SortKeys(body).Should().Equal("_score", EsCatalogFields.ReuseCount);

        // Descending, or the search returns its worst matches first.
        body.GetProperty("sort")[0].GetProperty("_score").GetString().Should().Be("desc");
    }

    [Test]
    public void A_resource_with_no_reuse_notion_sorts_last_among_equals()
    {
        // Absent is not zero. A dataset carries no reuse count at all, and must not be ordered as
        // though nobody reused it.
        ReuseSort(Body()).GetProperty("missing").GetString().Should().Be("_last");
    }

    [Test]
    public void The_most_reused_comes_first()
    {
        ReuseSort(Body()).GetProperty("order").GetString().Should().Be("desc");
    }

    [Test]
    public void What_a_resource_is_called_outweighs_what_it_says_about_itself()
    {
        // best_fields scores a document by its strongest field alone, so equal weights let a
        // description repeating the term beat an exact title. The ranking then never reaches
        // registration status or reuse, and a concept reused a hundred times loses to one reused
        // once. The exact numbers can be tuned; this ordering cannot be reversed.
        var fields = Body("statistik").GetProperty("query").GetProperty("function_score")
            .GetProperty("query").GetProperty("bool").GetProperty("must")[0].GetProperty("bool")
            .GetProperty("should").EnumerateArray()
            .First(x => x.TryGetProperty("multi_match", out _))
            .GetProperty("multi_match").GetProperty("fields").EnumerateArray()
            .Select(x => x.GetString()!)
            .ToDictionary(x => x.Split('^')[0], x => double.Parse(x.Split('^')[1], CultureInfo.InvariantCulture));

        fields["title.de"].Should().BeGreaterThan(fields["description.de"]);
        fields["name.de"].Should().BeGreaterThan(fields["description.de"]);

        // Someone pasting an identifier wants that exact resource, not whatever shares its words.
        fields["identifier.text"].Should().BeGreaterThanOrEqualTo(fields["title.de"]);
    }

    [Test]
    public void An_index_that_never_heard_of_reuse_is_still_searchable()
    {
        // Elasticsearch answers a sort on an unmapped field with a 400, not an empty result. The
        // live index has no such mapping until the first rebuild writes one, so leaving this out
        // means every catalogue search fails from the moment this is deployed until then.
        ReuseSort(Body()).GetProperty("unmapped_type").GetString().Should().Be("integer");

        ReuseSort(PointInTimeBody()).GetProperty("unmapped_type").GetString().Should().Be("integer");
    }

    // The export walk sorts the same way, and it would fail the same way.
    private static JsonElement PointInTimeBody() =>
        JsonDocument.Parse(JsonSerializer.Serialize(CatalogQueryBuilder.BuildPointInTimeBody(
            null, German, null, SearchCaller.Anonymous, "pit-1", TimeSpan.FromMinutes(1), 10, null)))
            .RootElement;

    private static string[] SortKeys(JsonElement body) =>
        [.. body.GetProperty("sort").EnumerateArray()
            .Select(x => x.EnumerateObject().Single().Name)];

    private static JsonElement ReuseSort(JsonElement body) =>
        body.GetProperty("sort").EnumerateArray()
            .Single(x => x.TryGetProperty(EsCatalogFields.ReuseCount, out _))
            .GetProperty(EsCatalogFields.ReuseCount);

    // 0.01 x the stored 85..110 gives the 0.85..1.10 multiplier the old engine applied.
    [Test]
    public void The_factor_reproduces_the_previous_multiplier()
    {
        var factor = Body().GetProperty("query").GetProperty("function_score")
            .GetProperty("field_value_factor");

        factor.GetProperty("field").GetString().Should().Be(EsCatalogFields.RegistrationStatusWeight);
        factor.GetProperty("factor").GetDouble().Should().Be(0.01);
    }

    [Test]
    public void A_missing_weight_is_a_no_op()
    {
        Body().GetProperty("query").GetProperty("function_score")
            .GetProperty("field_value_factor").GetProperty("missing").GetInt32()
            .Should().Be(100);
    }

    [Test]
    public void The_boost_wraps_the_query_rather_than_replacing_it()
    {
        var inner = Body("bev").GetProperty("query").GetProperty("function_score").GetProperty("query");

        inner.TryGetProperty("bool", out var boolQuery).Should().BeTrue();
        boolQuery.TryGetProperty("filter", out _).Should().BeTrue(
            because: "the authorization clause lives in the filter context and must survive the wrap");
    }

    [Test]
    public void The_count_body_is_not_boosted_because_it_scores_nothing()
    {
        var body = JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildCountBody(null, German, null, SearchCaller.Anonymous)["query"]);

        body.Should().NotContain("function_score");
    }
}
