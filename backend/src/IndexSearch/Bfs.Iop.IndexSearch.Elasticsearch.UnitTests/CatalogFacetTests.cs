using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogFacetTests
{
    private static readonly string[] German = ["de"];

    private static JsonElement Aggregations(CatalogSearchFilter? filter)
    {
        var body = CatalogQueryBuilder.BuildCountBody(null, German, filter, SearchCaller.Anonymous);

        return JsonDocument.Parse(JsonSerializer.Serialize(body["aggs"])).RootElement;
    }

    [Test]
    public void Every_dimension_is_aggregated()
    {
        var names = Aggregations(null).EnumerateObject()
            .Select(x => x.Name)
            .Where(x => x != CatalogFacetDimensions.Total)
            .ToArray();

        names.Should().BeEquivalentTo(CatalogQueryBuilder.Dimensions.Select(x => x.Dimension));
    }

    [Test]
    public void Publishers_bucket_on_the_case_preserving_field()
    {
        var publishers = Aggregations(null)
            .GetProperty(CatalogFacetDimensions.PublisherIdentifier)
            .GetProperty("aggs").GetProperty("values").GetProperty("terms").GetProperty("field").GetString();

        publishers.Should().Be(EsCatalogFields.PublisherIdentifierLabel);
    }

    [Test]
    public void A_dimension_excludes_its_own_selection_but_applies_the_others()
    {
        var filter = new CatalogSearchFilter
        {
            Themes = ["ENER"],
            Types = [IndexResourceType.Dataset],
        };

        var themes = JsonSerializer.Serialize(
            Aggregations(filter).GetProperty(CatalogFacetDimensions.Themes).GetProperty("filter"));

        themes.Should().NotContain("ENER", because: "the Themes facet must ignore the theme selection");
        themes.Should().Contain("Dataset", because: "but it must still respect every other selection");
    }

    [Test]
    public void The_structure_facet_is_aggregated_so_it_is_not_silently_empty()
    {
        Aggregations(null).TryGetProperty(CatalogFacetDimensions.HasStructure, out _).Should().BeTrue();
    }
}
