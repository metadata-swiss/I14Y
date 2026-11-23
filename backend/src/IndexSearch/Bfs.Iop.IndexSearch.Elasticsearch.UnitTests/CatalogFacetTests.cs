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
                CatalogFacetDimensions.TenantIdentifier => EsCatalogFields.TenantIdentifierLabel,
                CatalogFacetDimensions.PublisherIdentifier => EsCatalogFields.PublisherKeyLabel,
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
    public void The_publisher_facet_brings_back_the_publisher_identifier_and_name_with_each_bucket()
    {
        var topHits = Aggregations(null)
            .GetProperty(CatalogFacetDimensions.PublisherIdentifier)
            .GetProperty("aggs").GetProperty("values").GetProperty("aggs")
            .GetProperty(CatalogQueryBuilder.PublisherDetailsAggregationName).GetProperty("top_hits");

        topHits.GetProperty("size").GetInt32().Should().Be(1);
        topHits.GetProperty("_source").EnumerateArray().Select(x => x.GetString())
            .Should().Equal(EsCatalogFields.PublisherIdentifierLabel, EsCatalogFields.PublisherName);
    }

    [Test]
    public void The_tenant_and_publisher_selections_filter_their_own_fields_lowercased()
    {
        var total = JsonSerializer.Serialize(
            Aggregations(new CatalogSearchFilter { TenantIdentifiers = ["CH_BFS"], PublisherIdentifiers = ["CH_ESTV", " "] })
                .GetProperty(CatalogFacetDimensions.Total));

        total.Should().Contain($"\"{EsCatalogFields.TenantIdentifier}\":[\"ch_bfs\"]");
        total.Should().Contain($"\"{EsCatalogFields.PublisherKey}\":[\"ch_estv\"]");
    }

    [Test]
    public void The_tenant_selection_also_matches_documents_indexed_before_the_tenant_split_by_their_former_publisher()
    {
        var total = JsonSerializer.Serialize(
            Aggregations(new CatalogSearchFilter { TenantIdentifiers = ["CH_BFS"] })
                .GetProperty(CatalogFacetDimensions.Total));

        total.Should().Contain(
            "{\"must_not\":{\"exists\":{\"field\":\"tenant\"}},\"filter\":{\"terms\":{\"publisherIdentifier\":[\"ch_bfs\"]}}}");
    }

    [Test]
    public void The_publisher_facet_excludes_the_publisher_selection_but_applies_the_tenant_one()
    {
        var filter = new CatalogSearchFilter { TenantIdentifiers = ["CH_BFS"], PublisherIdentifiers = ["CH_ESTV"] };

        var publishers = JsonSerializer.Serialize(
            Aggregations(filter).GetProperty(CatalogFacetDimensions.PublisherIdentifier).GetProperty("filter"));

        publishers.Should().NotContain("ch_estv", because: "the Publishers facet must ignore the publisher selection");
        publishers.Should().Contain("ch_bfs", because: "but it must still respect the tenant selection");
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
            DefaultScoring.Catalog,
            from: 0,
            size: 25, CatalogSearchSort.Relevance, IndexLanguages.Default);

        var query = JsonDocument.Parse(JsonSerializer.Serialize(body["query"])).RootElement;

        var keywordTerms = query.GetProperty("bool").GetProperty("filter").EnumerateArray()
            .Where(x => x.TryGetProperty("terms", out var terms) && terms.TryGetProperty(EsCatalogFields.KeywordFilter, out _))
            .Select(x => x.GetProperty("terms").GetProperty(EsCatalogFields.KeywordFilter))
            .Single();

        keywordTerms.EnumerateArray().Select(x => x.GetString())
            .Should().BeEquivalentTo(["train", "bus"], because: "values are trimmed and blank values are dropped");
    }

    [Test]
    public void The_catalogues_facet_excludes_the_catalogue_selection_but_applies_the_others()
    {
        var catalog = Guid.NewGuid();

        var filter = new CatalogSearchFilter
        {
            DcatCatalogs = [catalog],
            Types = [SearchResourceType.Dataset],
        };

        var aggregations = Aggregations(filter);
        var dcatCatalogs = JsonSerializer.Serialize(aggregations.GetProperty(CatalogFacetDimensions.DcatCatalogs).GetProperty("filter"));
        var formats = JsonSerializer.Serialize(aggregations.GetProperty(CatalogFacetDimensions.Formats).GetProperty("filter"));

        dcatCatalogs.Should().NotContain(catalog.ToString(), because: "the DcatCatalogs facet must ignore the catalogue selection");
        dcatCatalogs.Should().Contain("Dataset", because: "but it must still respect every other selection");
        formats.Should().Contain(catalog.ToString(), because: "every other facet counts within the selected catalogues");
    }

    [Test]
    public void The_search_query_filters_on_the_selected_catalogues()
    {
        var catalog = Guid.NewGuid();

        var body = CatalogQueryBuilder.BuildSearchBody(
            null,
            German,
            new CatalogSearchFilter { DcatCatalogs = [catalog] },
            SearchCaller.Anonymous,
            DefaultScoring.Catalog,
            from: 0,
            size: 25, CatalogSearchSort.Relevance, IndexLanguages.Default);

        var query = JsonDocument.Parse(JsonSerializer.Serialize(body["query"])).RootElement;

        query.GetProperty("bool").GetProperty("filter").EnumerateArray()
            .Where(x => x.TryGetProperty("terms", out var terms) && terms.TryGetProperty(EsCatalogFields.DcatCatalogs, out _))
            .Select(x => x.GetProperty("terms").GetProperty(EsCatalogFields.DcatCatalogs))
            .Single()
            .EnumerateArray().Select(x => x.GetString())
            .Should().Equal(catalog.ToString());
    }
}
