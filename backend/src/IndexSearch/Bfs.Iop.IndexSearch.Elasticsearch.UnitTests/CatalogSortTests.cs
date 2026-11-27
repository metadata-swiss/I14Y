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
        ["_score", EsCatalogFields.RegistrationStatusWeight, EsCatalogFields.VersionSort, EsCatalogFields.ReuseCount];

    [Test]
    public void Relevance_orders_by_score_then_status_then_version_then_reuse()
    {
        SortKeys(Body(CatalogSearchSort.Relevance)).Should().Equal(RelevanceChain);
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
    public void A_long_title_sorts_by_its_beginning_instead_of_as_missing()
    {
        var title = "A" + new string('b', CatalogDocumentFactory.TitleSortMaxLength);

        var titleSort = TitleSortOf(new MultiLanguageModel { En = title });

        titleSort["en"].Should().Be(title[..CatalogDocumentFactory.TitleSortMaxLength]);
    }

    [Test]
    public void A_long_title_is_not_cut_through_a_character()
    {
        var title = new string('a', CatalogDocumentFactory.TitleSortMaxLength - 1) + "😀" + "zzz";

        var sortTitle = (string)TitleSortOf(new MultiLanguageModel { En = title })["en"]!;

        sortTitle.Should().Be(new string('a', CatalogDocumentFactory.TitleSortMaxLength - 1));
    }

    [Test]
    public void Among_equal_matches_the_newest_version_comes_first()
    {
        var version = SortOn(Body(CatalogSearchSort.Relevance), EsCatalogFields.VersionSort);

        version.GetProperty("order").GetString().Should().Be("desc");
        version.GetProperty("missing").GetString().Should().Be("_last");
        version.GetProperty("unmapped_type").GetString().Should().Be("keyword");
    }

    [Test]
    public void Versions_compare_part_by_part_as_numbers()
    {
        string[] versions = ["2020.3.0", "2024.1.0", "2022.10.0", "2022.9.1", "2022.2.0", "2022.10", "9.0.0"];

        versions.OrderByDescending(CatalogDocumentFactory.VersionSortKey, StringComparer.Ordinal)
            .Should().Equal("2024.1.0", "2022.10.0", "2022.10", "2022.9.1", "2022.2.0", "2020.3.0", "9.0.0");
    }

    [TestCase("2024.1.0", "0000002024.0000000001.0000000000")]
    [TestCase(" 1.02 ", "0000000001.0000000002")]
    [TestCase("v2.1-beta", "0000000002.0000000001")]
    [TestCase("1.x", "0000000001.0000000000")]
    public void A_version_becomes_a_key_of_zero_padded_numbers(string version, string key)
    {
        CatalogDocumentFactory.VersionSortKey(version).Should().Be(key);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("draft")]
    public void A_version_without_digits_has_no_key_and_sorts_last(string? version)
    {
        CatalogDocumentFactory.VersionSortKey(version).Should().BeNull();
        CatalogDocumentFactory.Build(Entry(new MultiLanguageModel { De = "x" }) with { Version = version },
                DefaultScoring.Catalog).Body
            .Should().NotContainKey(EsCatalogFields.VersionSort);
    }

    [Test]
    public void An_indexed_resource_carries_its_version_key()
    {
        CatalogDocumentFactory.Build(Entry(new MultiLanguageModel { De = "x" }) with { Version = "2022.2.0" },
                DefaultScoring.Catalog).Body[EsCatalogFields.VersionSort]
            .Should().Be("0000002022.0000000002.0000000000");
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
    public void A_resource_without_a_title_keeps_its_place_in_the_export_walk()
    {
        using var response = JsonDocument.Parse(
            """{"hits":{"hits":[{"_source":{"id":"11111111-1111-1111-1111-111111111111","type":"Dataset"},"sort":[null,1.5,"a"]}]}}""");

        var cursor = CatalogResponseReader.ReadPage(response.RootElement).Single().Cursor;

        cursor.Should().Equal(null, 1.5, "a");
    }

    [Test]
    public void The_export_walk_continues_after_a_resource_without_a_title()
    {
        var body = Parse(CatalogQueryBuilder.BuildPointInTimeBody(
            null, AllLanguages, null, SearchCaller.Anonymous, DefaultScoring.Catalog, "pit-1",
            TimeSpan.FromMinutes(1), 10, [null, 1.5, "a"], CatalogSearchSort.TitleAscending, "en"));

        body.GetProperty("search_after")[0].ValueKind.Should().Be(JsonValueKind.Null);
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
            key.TryGetProperty("ignore_above", out _).Should().BeFalse();
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

    private static JsonElement PointInTimeBody(CatalogSearchSort sort, string sortLanguage) =>
        Parse(CatalogQueryBuilder.BuildPointInTimeBody(
            null, AllLanguages, null, SearchCaller.Anonymous, DefaultScoring.Catalog, "pit-1",
            TimeSpan.FromMinutes(1), 10, null, sort, sortLanguage));

    private static JsonElement Parse(object body) => JsonDocument.Parse(JsonSerializer.Serialize(body)).RootElement;

    private static string[] SortKeys(JsonElement body) =>
        [.. body.GetProperty("sort").EnumerateArray().Select(x => x.EnumerateObject().Single().Name)];

    private static JsonElement SortOn(JsonElement body, string field) =>
        body.GetProperty("sort").EnumerateArray()
            .Single(x => x.TryGetProperty(field, out _))
            .GetProperty(field);
}
