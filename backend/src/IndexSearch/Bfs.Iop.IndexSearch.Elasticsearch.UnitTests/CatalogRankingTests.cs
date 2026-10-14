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
        var body = Body();

        body.TryGetProperty("sort", out _).Should().BeFalse(
            because: "ranking is by score alone; status is folded into the score, not applied after it");

        var function = body.GetProperty("query").GetProperty("function_score");

        function.GetProperty("boost_mode").GetString().Should().Be("multiply");
    }

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
