using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Helpers;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.LinkedData.UnitTests;

[TestFixture(TestOf = typeof(ShaclSparqlQueryHelper))]
internal sealed class ShaclSparqlQueryHelperTests
{
    private static readonly SparqlQueryParser Parser = new();

    [Test]
    public void Given_valid_concept_iri_When_building_count_query_Then_query_is_parseable_and_contains_the_iri()
    {
        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery(
            ["https://example.org/i14y/concept/c1/version/1.0.0"]);

        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        query.Should().Contain("<https://example.org/i14y/concept/c1/version/1.0.0>");
    }

    [Test]
    public void Given_malicious_identifier_When_building_count_query_Then_query_stays_parseable_and_is_not_injected()
    {
        // A client-supplied identifier crafted to break out of the <...> IRI token and inject SPARQL.
        var malicious = "https://example.org/i14y/concept/x> } } INSERT DATA { <a> <b> <c> } #/version/1";

        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery([malicious]);

        // The query must remain syntactically valid (no break-out) ...
        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        // ... and must not contain the raw injected SPARQL keyword as executable text.
        query.Should().NotContain("INSERT DATA");
    }

    [Test]
    public void Given_non_absolute_iri_When_building_count_query_Then_it_is_skipped_and_query_is_parseable()
    {
        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery(["not-an-absolute-uri"]);

        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        query.Should().NotContain("not-an-absolute-uri");
    }
}
