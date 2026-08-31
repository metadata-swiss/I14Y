using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogQueryBuilderReviewFixTests
{
    private static readonly string[] German = ["de"];

    private static JsonElement Body(int from, int size) =>
        JsonDocument.Parse(JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(null, German, null, SearchCaller.Anonymous, from, size)))
            .RootElement;

    [Test]
    public void A_page_size_beyond_the_result_window_is_clamped()
    {
        var body = Body(from: 0, size: int.MaxValue);

        body.GetProperty("size").GetInt32().Should().Be(CatalogQueryBuilder.MaxResultWindow);
    }

    [Test]
    public void From_plus_size_never_exceeds_the_result_window()
    {
        var body = Body(from: 9_900, size: 500);

        (body.GetProperty("from").GetInt32() + body.GetProperty("size").GetInt32())
            .Should().BeLessThanOrEqualTo(CatalogQueryBuilder.MaxResultWindow);
    }

    [Test]
    public void Ordinary_paging_is_untouched()
    {
        var body = Body(from: 20, size: 10);

        body.GetProperty("from").GetInt32().Should().Be(20);
        body.GetProperty("size").GetInt32().Should().Be(10);
    }

    [Test]
    public void Email_terms_are_lowercased_to_match_the_indexed_token()
    {
        var query = JsonSerializer.Serialize(CatalogQueryBuilder.BuildSearchBody(
            "Ada@Example.CH", German, null, SearchCaller.Anonymous, 0, 10)["query"]);

        foreach (var emailField in new[] { "responsiblePersonEmail", "responsibleDeputyEmail", "contactPointHasEmail", "channelEmail" })
        {
            query.Should().Contain($"\"{emailField}\":\"ada@example.ch\"");
        }
    }

    [Test]
    public void The_total_aggregation_applies_every_selection()
    {
        var filter = new CatalogSearchFilter
        {
            Themes = ["ENER"],
            Types = [IndexResourceType.Dataset],
        };

        var aggs = JsonDocument.Parse(JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildCountBody(null, German, filter, SearchCaller.Anonymous)["aggs"])).RootElement;

        var total = JsonSerializer.Serialize(aggs.GetProperty(CatalogFacetDimensions.Total));

        total.Should().Contain("ENER");
        total.Should().Contain("Dataset");
    }

    [Test]
    public void The_total_aggregation_is_not_a_facet_dimension()
    {
        CatalogQueryBuilder.Dimensions.Select(x => x.Dimension)
            .Should().NotContain(CatalogFacetDimensions.Total);
    }
}
