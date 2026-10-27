using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogSearchCoverageTests
{
    private static readonly string[] German = ["de"];

    // A caller who may search every field, so these assert coverage rather than public exposure -
    // PublicSearchFieldExposureTests covers what an anonymous caller is kept away from.
    private static readonly SearchCaller Privileged =
        new() { HasValidToken = true, Role = Bfs.Iop.Infrastructure.Security.BusinessRole.SwissDataSteward };

    private static JsonElement Query(string queryString) =>
        JsonDocument.Parse(JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(queryString, German, null, Privileged, 0, 10)))
            .RootElement.GetProperty("query");

    private static JsonElement FreeText(string queryString) =>
        Query(queryString).GetProperty("function_score").GetProperty("query")
            .GetProperty("bool").GetProperty("must")[0].GetProperty("bool");

    private static bool IsMultiMatch(JsonElement clause) => clause.TryGetProperty("multi_match", out _);

    private static bool IsTerm(JsonElement clause) => clause.TryGetProperty("term", out _);

    private static IEnumerable<JsonElement> Should(string queryString) =>
        FreeText(queryString).GetProperty("should").EnumerateArray();

    private static string[] FieldsOf(JsonElement multiMatch) =>
        [.. multiMatch.GetProperty("multi_match").GetProperty("fields")
            .EnumerateArray().Select(x => x.GetString()!)];

    private static JsonElement WholeWordMatch(string queryString) =>
        Should(queryString).Where(IsMultiMatch)
            .Single(x => !x.GetProperty("multi_match").TryGetProperty("boost", out _));

    private static JsonElement PartialMatch(string queryString) =>
        Should(queryString).Where(IsMultiMatch)
            .Single(x => x.GetProperty("multi_match").TryGetProperty("boost", out _));

    [Test]
    public void Free_text_reaches_every_analysed_field()
    {
        // The weights are part of the contract: best_fields scores a document by its strongest
        // field alone, so an unweighted description would outrank an exact title and the ranking
        // would never reach registration status or reuse.
        FieldsOf(WholeWordMatch("statistik")).Should().BeEquivalentTo(
        [
            "title.de^5",
            "name.de^5",
            "description.de^0.3",
            "keyword.de^1",
            "contactPointFn.de^0.2",
            "contactPointHasAddress.de^0.2",
            "contactPointNote.de^0.2",
            "identifier.text^5",
            "dataOwner.text^0.2",
            "version.text^5",
            "responsiblePersonName.text^0.2",
            "responsibleDeputyName.text^0.2",
            "contactPointHasTelephone.text^0.2",
        ]);
    }

    [Test]
    public void Partial_terms_are_searched_on_the_ngram_fields_only()
    {
        // Weighted the same way as the whole-word clause, or a partial description match would
        // outrank a partial title match.
        FieldsOf(PartialMatch("statis")).Should().BeEquivalentTo(
        [
            "title.de.ngram^5",
            "name.de.ngram^5",
            "description.de.ngram^0.3",
            "keyword.de.ngram^1",
            "contactPointFn.de.ngram^0.2",
            "contactPointHasAddress.de.ngram^0.2",
            "contactPointNote.de.ngram^0.2",
        ]);
    }

    [Test]
    public void Partial_matches_score_below_whole_word_matches_and_need_most_of_the_grams()
    {
        var partial = PartialMatch("statis").GetProperty("multi_match");

        partial.GetProperty("boost").GetDouble().Should().Be(0.75);
        partial.GetProperty("minimum_should_match").GetString().Should().Be("75%");
    }

    [Test]
    public void Both_clauses_search_the_query_without_its_surrounding_quotes()
    {
        WholeWordMatch("\"statistik\"").GetProperty("multi_match")
            .GetProperty("query").GetString().Should().Be("statistik");

        PartialMatch("\"statistik\"").GetProperty("multi_match")
            .GetProperty("query").GetString().Should().Be("statistik");
    }

    [Test]
    public void Free_text_matches_the_single_token_fields_exactly()
    {
        var terms = Should("Statistik")
            .Where(IsTerm)
            .SelectMany(x => x.GetProperty("term").EnumerateObject())
            .ToDictionary(x => x.Name, x => x.Value.GetString());

        terms.Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["id"] = "statistik",
            ["responsiblePersonEmail"] = "statistik",
            ["responsibleDeputyEmail"] = "statistik",
            ["contactPointHasEmail"] = "statistik",
            ["channelEmail"] = "statistik",
        });
    }

    [TestCase("ada@example.ch", TestName = "bare")]
    [TestCase("  \"ada@example.ch\"  ", TestName = "quoted by the admin UI")]
    public void An_address_on_its_own_is_matched_against_the_address_fields_only(string queryString)
    {
        var clauses = Should(queryString).ToArray();


        clauses.Should().NotContain(x => IsMultiMatch(x));

        clauses.SelectMany(x => x.GetProperty("term").EnumerateObject())
            .Select(x => x.Name)
            .Should().BeEquivalentTo(
                ["responsiblePersonEmail", "responsibleDeputyEmail", "contactPointHasEmail", "channelEmail"]);
    }

    [Test]
    public void A_query_that_merely_contains_an_address_still_searches_everything()
    {

        Should("kontakt ada@example.ch").Should().Contain(x => IsMultiMatch(x));
    }

    [Test]
    public void Every_identifier_a_resource_has_is_indexed()
    {
        var document = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.DataService,
            Identifiers = ["ds-1", "ds-1-superseded"],
        });


        document.Body["identifier"].Should().BeEquivalentTo(new[] { "ds-1", "ds-1-superseded" });
    }

    [Test]
    public void A_contact_telephone_is_indexed()
    {
        var document = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.Dataset,
            ContactPoints = [new IndexContactPoint { HasTelephone = "+41 58 000 00 00" }],
        });

        document.Body["contactPointHasTelephone"].Should().BeEquivalentTo(new[] { "+41 58 000 00 00" });
    }

    [Test]
    public void A_searchable_keyword_keeps_an_exact_root_and_gains_an_analysed_copy()
    {
        using var mapping = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());

        var identifier = mapping.RootElement
            .GetProperty("mappings").GetProperty("properties").GetProperty("identifier");

        identifier.GetProperty("type").GetString().Should().Be("keyword");

        var text = identifier.GetProperty("fields").GetProperty("text");
        text.GetProperty("type").GetString().Should().Be("text");
        text.GetProperty("analyzer").GetString().Should().Be("i14y_text");
    }
}
