using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
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

    // Paging.ToWindow owns the result-window rule and is tested there. What the builder owes is the
    // opposite guarantee: that it passes the window through untouched rather than clamping again.
    [Test]
    public void The_window_reaches_the_body_untouched()
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
            Types = [SearchResourceType.Dataset],
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
