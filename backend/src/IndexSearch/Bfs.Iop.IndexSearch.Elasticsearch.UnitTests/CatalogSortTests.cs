using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogSortTests
{
    private static readonly string[] AllLanguages = ["de", "en", "fr", "it", "rm"];

    private static readonly string[] RelevanceChain =
        ["_score", EsCatalogFields.RegistrationStatusWeight, EsCatalogFields.ReuseCount];

    [Test]
    public void Relevance_is_the_default_order()
    {
        Serialize(Body(CatalogSearchSort.Relevance).GetProperty("sort"))
            .Should().Be(Serialize(DefaultBody().GetProperty("sort")));
    }

    [Test]
    public void Title_sort_orders_the_whole_result_by_one_title_per_resource_then_relevance()
    {
        SortKeys(Body(CatalogSearchSort.TitleAscending, "fr")).Should().Equal(
            ["titleSort.fr", .. RelevanceChain]);
    }

    [TestCase(CatalogSearchSort.TitleAscending, "asc")]
    [TestCase(CatalogSearchSort.TitleDescending, "desc")]
    public void Title_sort_orders_in_the_requested_direction(CatalogSearchSort sort, string order)
    {
        SortOn(Body(sort, "fr"), "titleSort.fr").GetProperty("order").GetString().Should().Be(order);
    }

    [Test]
    public void A_resource_sorts_by_its_title_in_each_language_it_has_one()
    {
        var titleSort = TitleSortOf(new MultiLanguageModel { De = "Gebäude", Fr = "Bâtiments" });

        titleSort["de"].Should().Be("Gebäude");
        titleSort["fr"].Should().Be("Bâtiments");
    }

    [Test]
    public void A_resource_without_a_title_in_a_language_sorts_there_by_its_german_one()
    {
        var titleSort = TitleSortOf(new MultiLanguageModel { De = "Gebäude", Fr = "Bâtiments" });

        titleSort["en"].Should().Be("Gebäude");
        titleSort["it"].Should().Be("Gebäude");
        titleSort["rm"].Should().Be("Gebäude");
    }

    [Test]
    public void A_resource_without_a_german_title_falls_back_to_the_next_language_it_has()
    {
        var titleSort = TitleSortOf(new MultiLanguageModel { Fr = "Bâtiments", It = "Edifici" });

        titleSort["de"].Should().Be("Bâtiments");
        titleSort["en"].Should().Be("Bâtiments");
        titleSort["it"].Should().Be("Edifici");
    }

    [Test]
    public void A_title_sorts_without_its_surrounding_spaces()
    {
        var titleSort = TitleSortOf(new MultiLanguageModel { De = " hga_test_dataset " });

        titleSort["de"].Should().Be("hga_test_dataset");
        titleSort["en"].Should().Be("hga_test_dataset");
    }

    [Test]
    public void A_resource_without_any_title_has_no_sort_title()
    {
        CatalogDocumentFactory.Build(Entry(new MultiLanguageModel()), DefaultScoring.Catalog).Body
            .Should().NotContainKey(EsCatalogFields.TitleSort);
    }

    [TestCase(CatalogSearchSort.ModifiedNewest, "desc")]
    [TestCase(CatalogSearchSort.ModifiedOldest, "asc")]
    public void Modified_sort_leads_with_the_modification_date(CatalogSearchSort sort, string order)
    {
        var body = Body(sort);

        SortKeys(body).Should().Equal([EsCatalogFields.ModifiedAt, .. RelevanceChain]);
        SortOn(body, EsCatalogFields.ModifiedAt).GetProperty("order").GetString().Should().Be(order);
    }

    [TestCase(CatalogSearchSort.TitleAscending, "keyword")]
    [TestCase(CatalogSearchSort.TitleDescending, "keyword")]
    [TestCase(CatalogSearchSort.ModifiedNewest, "date")]
    [TestCase(CatalogSearchSort.ModifiedOldest, "date")]
    public void An_index_built_before_the_sort_key_existed_is_still_searchable(CatalogSearchSort sort, string unmappedType)
    {
        var lead = Body(sort, "fr").GetProperty("sort")[0].EnumerateObject().Single().Value;

        lead.GetProperty("unmapped_type").GetString().Should().Be(unmappedType);
        lead.GetProperty("missing").GetString().Should().Be("_last");
    }

    [TestCase(CatalogSearchSort.Relevance)]
    [TestCase(CatalogSearchSort.TitleAscending)]
    [TestCase(CatalogSearchSort.TitleDescending)]
    [TestCase(CatalogSearchSort.ModifiedNewest)]
    [TestCase(CatalogSearchSort.ModifiedOldest)]
    public void The_export_walk_sorts_like_the_search_and_ends_on_the_shard_doc(CatalogSearchSort sort)
    {
        var search = SortKeys(Body(sort, "fr"));

        SortKeys(PointInTimeBody(sort, "fr")).Should().Equal([.. search, "_shard_doc"]);
    }

    [Test]
    public void The_sort_title_is_a_case_and_accent_insensitive_keyword_in_every_language()
    {
        using var doc = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());
        var titleSort = doc.RootElement.GetProperty("mappings").GetProperty("properties")
            .GetProperty(EsCatalogFields.TitleSort).GetProperty("properties");

        foreach (var language in AllLanguages)
        {
            var key = titleSort.GetProperty(language);

            key.GetProperty("type").GetString().Should().Be("keyword");
            key.GetProperty("normalizer").GetString().Should().Be(EsAnalysis.LowercaseNormalizer);
        }
    }

    private static Dictionary<string, object?> TitleSortOf(MultiLanguageModel title) =>
        (Dictionary<string, object?>)CatalogDocumentFactory.Build(Entry(title), DefaultScoring.Catalog)
            .Body[EsCatalogFields.TitleSort]!;

    private static CatalogIndexDocument Entry(MultiLanguageModel title) => new()
    {
        Id = Guid.NewGuid(),
        Type = SearchResourceType.Dataset,
        Title = title,
    };

    private static JsonElement Body(CatalogSearchSort sort, string sortLanguage = "de") =>
        Parse(CatalogQueryBuilder.BuildSearchBody(
            null, AllLanguages, null, SearchCaller.Anonymous, DefaultScoring.Catalog, from: 0, size: 10, sort, sortLanguage));

    private static JsonElement DefaultBody() =>
        Parse(CatalogQueryBuilder.BuildSearchBody(
            null, AllLanguages, null, SearchCaller.Anonymous, DefaultScoring.Catalog, from: 0, size: 10));

    private static JsonElement PointInTimeBody(CatalogSearchSort sort, string sortLanguage) =>
        Parse(CatalogQueryBuilder.BuildPointInTimeBody(
            null, AllLanguages, null, SearchCaller.Anonymous, DefaultScoring.Catalog, "pit-1",
            TimeSpan.FromMinutes(1), 10, null, sort, sortLanguage));

    private static JsonElement Parse(object body) => JsonDocument.Parse(JsonSerializer.Serialize(body)).RootElement;

    private static string Serialize(JsonElement element) => element.GetRawText();

    private static string[] SortKeys(JsonElement body) =>
        [.. body.GetProperty("sort").EnumerateArray().Select(x => x.EnumerateObject().Single().Name)];

    private static JsonElement SortOn(JsonElement body, string field) =>
        body.GetProperty("sort").EnumerateArray()
            .Single(x => x.TryGetProperty(field, out _))
            .GetProperty(field);
}
