using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
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
        var body = CatalogQueryBuilder.BuildCountBody(null, German, filter, SearchCaller.Anonymous, DefaultScoring.Catalog);

        return JsonDocument.Parse(JsonSerializer.Serialize(body["aggs"])).RootElement;
    }

    [Test]
    public void Every_dimension_aggregates_the_field_its_name_implies()
    {
        var aggregations = Aggregations(null);

        foreach (var (dimension, _) in CatalogQueryBuilder.Dimensions)
        {
            var expected = dimension switch
            {
                CatalogFacetDimensions.PublisherIdentifier => EsCatalogFields.PublisherIdentifierLabel,
                CatalogFacetDimensions.Keywords => EsCatalogFields.KeywordFilter,
                _ => char.ToLowerInvariant(dimension[0]) + dimension[1..],
            };

            aggregations
                .GetProperty(dimension)
                .GetProperty("aggs").GetProperty("values").GetProperty("terms").GetProperty("field")
                .GetString()
                .Should().Be(expected, because: "the {0} facet must bucket on {1}", dimension, expected);
        }
    }

    [Test]
    public void A_dimension_excludes_its_own_selection_but_applies_the_others()
    {
        var filter = new CatalogSearchFilter
        {
            Themes = ["ENER"],
            Types = [SearchResourceType.Dataset],
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

    [Test]
    public void The_keywords_facet_is_aggregated_on_the_language_independent_keyword_field()
    {
        Aggregations(null)
            .GetProperty(CatalogFacetDimensions.Keywords)
            .GetProperty("aggs").GetProperty("values").GetProperty("terms").GetProperty("field")
            .GetString()
            .Should().Be(EsCatalogFields.KeywordFilter,
                because: "counting on the field that merges every language counts a resource once per keyword");
    }

    [Test]
    public void The_keywords_facet_excludes_the_keyword_selection_but_applies_the_others()
    {
        var filter = new CatalogSearchFilter
        {
            Keywords = ["train"],
            Types = [SearchResourceType.Dataset],
        };

        var keywords = JsonSerializer.Serialize(
            Aggregations(filter).GetProperty(CatalogFacetDimensions.Keywords).GetProperty("filter"));

        keywords.Should().NotContain("train", because: "the Keywords facet must ignore the keyword selection");
        keywords.Should().Contain("Dataset", because: "but it must still respect every other selection");
    }

    [Test]
    public void The_total_aggregation_applies_the_keyword_selection()
    {
        var total = JsonSerializer.Serialize(
            Aggregations(new CatalogSearchFilter { Keywords = ["train"] }).GetProperty(CatalogFacetDimensions.Total));

        total.Should().Contain(EsCatalogFields.KeywordFilter);
        total.Should().Contain("train");
    }

    [Test]
    public void The_search_query_filters_keywords_on_the_language_independent_field_trimmed_and_without_blanks()
    {
        var body = CatalogQueryBuilder.BuildSearchBody(
            null,
            German,
            new CatalogSearchFilter { Keywords = [" train ", " ", "bus"] },
            SearchCaller.Anonymous,
            from: 0,
            size: 25);

        var query = JsonDocument.Parse(JsonSerializer.Serialize(body["query"])).RootElement;

        var keywordTerms = query.GetProperty("bool").GetProperty("filter").EnumerateArray()
            .Where(x => x.TryGetProperty("terms", out var terms) && terms.TryGetProperty(EsCatalogFields.KeywordFilter, out _))
            .Select(x => x.GetProperty("terms").GetProperty(EsCatalogFields.KeywordFilter))
            .Single();

        keywordTerms.EnumerateArray().Select(x => x.GetString())
            .Should().BeEquivalentTo(["train", "bus"], because: "values are trimmed and blank values are dropped");
    }
}
