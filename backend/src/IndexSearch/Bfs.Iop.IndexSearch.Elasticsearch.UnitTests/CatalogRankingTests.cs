using System.Globalization;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.Infrastructure.Security;
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
    public void Relevance_leads_then_status_then_reuse()
    {
        // The whole business rule in one assertion: how well the text matched, then how settled the
        // resource is, then how much it is reused.
        var body = Body();

        SortKeys(body).Should().Equal(
            "_score",
            EsCatalogFields.RegistrationStatusWeight,
            EsCatalogFields.ReuseCount);

        // Descending, or the search returns its worst matches first.
        body.GetProperty("sort")[0].GetProperty("_score").GetString().Should().Be("desc");
    }

    [Test]
    public void Every_business_role_is_ordered_the_same_way()
    {
        // The requirement in one test. Walked over the enum rather than a hand-written list, so a
        // role added later cannot quietly inherit a different order. What each role is allowed to
        // see still differs - that is the authorization clause, and it belongs in the query, not
        // in how the results that survive it are ranked.
        var orders = Enum.GetValues<BusinessRole>()
            .Select(role => new SearchCaller { HasValidToken = role is not BusinessRole.Unknown, Role = role })
            .Select(caller => JsonSerializer.Serialize(
                CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, from: 0, size: 10)["sort"]))
            .Distinct()
            .ToArray();

        orders.Should().ContainSingle();
    }

    [Test]
    public void Status_orders_results_for_every_role_not_only_the_two_that_see_everything()
    {
        // Status used to multiply the score. Only InteroperabilityService and SwissDataSteward have
        // no authorization clause, so only their query degrades to match_all and scores 1; everyone
        // else gets a filter, filters do not score, and multiplying zero left status inert. As a
        // sort key it orders results whatever the score was - which is why it must not go back.
        Body().GetProperty("query").TryGetProperty("function_score", out _).Should().BeFalse();

        StatusSort(Body()).GetProperty("order").GetString().Should().Be("desc");
    }

    [Test]
    public void A_status_the_index_never_stored_ranks_as_Recorded()
    {
        // What the old multiplier assumed with its missing: 100.
        StatusSort(Body()).GetProperty("missing").GetInt32().Should().Be(100);
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
        var fields = Body("statistik").GetProperty("query").GetProperty("bool").GetProperty("must")[0].GetProperty("bool")
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
        StatusSort(Body()).GetProperty("unmapped_type").GetString().Should().Be("integer");

        ReuseSort(PointInTimeBody()).GetProperty("unmapped_type").GetString().Should().Be("integer");
        StatusSort(PointInTimeBody()).GetProperty("unmapped_type").GetString().Should().Be("integer");
    }

    // The export walk sorts the same way, and it would fail the same way.
    private static JsonElement PointInTimeBody() =>
        JsonDocument.Parse(JsonSerializer.Serialize(CatalogQueryBuilder.BuildPointInTimeBody(
            null, German, null, SearchCaller.Anonymous, "pit-1", TimeSpan.FromMinutes(1), 10, null)))
            .RootElement;

    private static string[] SortKeys(JsonElement body) =>
        [.. body.GetProperty("sort").EnumerateArray()
            .Select(x => x.EnumerateObject().Single().Name)];

    private static JsonElement ReuseSort(JsonElement body) => SortOn(body, EsCatalogFields.ReuseCount);

    private static JsonElement StatusSort(JsonElement body) =>
        SortOn(body, EsCatalogFields.RegistrationStatusWeight);

    private static JsonElement SortOn(JsonElement body, string field) =>
        body.GetProperty("sort").EnumerateArray()
            .Single(x => x.TryGetProperty(field, out _))
            .GetProperty(field);
}
