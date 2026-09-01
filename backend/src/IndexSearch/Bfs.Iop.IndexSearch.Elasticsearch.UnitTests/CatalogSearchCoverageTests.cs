using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// What free text has to reach. Written as literals rather than through EsCatalogFields on purpose: a
// test built from the same constants as the code passes even when a field is renamed out from under
// both front-ends, or dropped from the query entirely, which is how the gap this pins went unnoticed.
[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogSearchCoverageTests
{
    private static readonly string[] German = ["de"];

    private static JsonElement Query(string queryString) =>
        JsonDocument.Parse(JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(queryString, German, null, SearchCaller.Anonymous, 0, 10)))
            .RootElement.GetProperty("query");

    // function_score wraps the bool; the free-text clause is the only "must".
    private static JsonElement FreeText(string queryString) =>
        Query(queryString).GetProperty("function_score").GetProperty("query")
            .GetProperty("bool").GetProperty("must")[0].GetProperty("bool");

    private static bool IsMultiMatch(JsonElement clause) => clause.TryGetProperty("multi_match", out _);

    private static bool IsTerm(JsonElement clause) => clause.TryGetProperty("term", out _);

    private static IEnumerable<JsonElement> Should(string queryString) =>
        FreeText(queryString).GetProperty("should").EnumerateArray();

    [Test]
    public void Free_text_reaches_every_analysed_field()
    {
        var fields = Should("statistik")
            .Single(IsMultiMatch)
            .GetProperty("multi_match").GetProperty("fields")
            .EnumerateArray().Select(x => x.GetString()).ToArray();

        fields.Should().BeEquivalentTo(
        [
            "title.de", "title.de.ngram",
            "name.de", "name.de.ngram",
            "description.de", "description.de.ngram",
            "keyword.de", "keyword.de.ngram",
            "contactPointFn.de", "contactPointFn.de.ngram",
            "contactPointHasAddress.de", "contactPointHasAddress.de.ngram",
            "contactPointNote.de", "contactPointNote.de.ngram",

            // The ones Lucene searched as text and this index had mapped as bare keywords, so that
            // typing an identifier or a data owner found nothing at all.
            "identifier.text",
            "dataOwner.text",
            "version.text",
            "responsiblePersonName.text",
            "responsibleDeputyName.text",
            "contactPointHasTelephone.text",
        ]);
    }

    [Test]
    public void Free_text_matches_the_single_token_fields_exactly()
    {
        var terms = Should("Statistik")
            .Where(IsTerm)
            .SelectMany(x => x.GetProperty("term").EnumerateObject())
            .ToDictionary(x => x.Name, x => x.Value.GetString());

        // A pasted id, and the addresses. Lowercased, because that is how they are indexed.
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

        // The analysed fields would split ada@example.ch into ada/example/ch and return everything
        // containing "ch" (bug #725). No multi_match may survive here.
        clauses.Should().NotContain(x => IsMultiMatch(x));

        clauses.SelectMany(x => x.GetProperty("term").EnumerateObject())
            .Select(x => x.Name)
            .Should().BeEquivalentTo(
                ["responsiblePersonEmail", "responsibleDeputyEmail", "contactPointHasEmail", "channelEmail"]);
    }

    [Test]
    public void A_query_that_merely_contains_an_address_still_searches_everything()
    {
        // The short-circuit is for a query that is nothing but an address; anything longer is ordinary
        // free text and must not lose the analysed fields.
        Should("kontakt ada@example.ch").Should().Contain(x => IsMultiMatch(x));
    }

    [Test]
    public void Every_identifier_a_resource_has_is_indexed()
    {
        var (_, document) = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = IndexResourceType.DataService,
            Identifiers = ["ds-1", "ds-1-superseded"],
        });

        // Only the first used to travel, so a resource renamed at some point could not be found by the
        // identifier people still had written down.
        document["identifier"].Should().BeEquivalentTo(new[] { "ds-1", "ds-1-superseded" });
    }

    [Test]
    public void A_contact_telephone_is_indexed()
    {
        var (_, document) = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = IndexResourceType.Dataset,
            ContactPoints = [new IndexContactPoint { HasTelephone = "+41 58 000 00 00" }],
        });

        // Lucene indexed and searched it; the document had nowhere to put it.
        document["contactPointHasTelephone"].Should().BeEquivalentTo(new[] { "+41 58 000 00 00" });
    }

    [Test]
    public void A_searchable_keyword_keeps_an_exact_root_and_gains_an_analysed_copy()
    {
        using var mapping = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());

        var identifier = mapping.RootElement
            .GetProperty("mappings").GetProperty("properties").GetProperty("identifier");

        // Exact at the root so filters, facets and sorts keep seeing one token...
        identifier.GetProperty("type").GetString().Should().Be("keyword");

        // ...and analysed underneath so free text can match part of it, in any case.
        var text = identifier.GetProperty("fields").GetProperty("text");
        text.GetProperty("type").GetString().Should().Be("text");
        text.GetProperty("analyzer").GetString().Should().Be("i14y_text");
    }
}
