using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.Search.Elasticsearch.UnitTests;

/// <summary>
/// Pins the two-field split on the publisher identifier.
/// <para>
/// The publishers facet cannot bucket on the lowercased field. Bucket keys are handed to
/// <c>IAgentReader.GetAgents(IEnumerable&lt;string&gt;)</c>, which resolves them with a
/// case-sensitive <c>= ANY(…)</c> against <c>agents.identifier</c>; identifiers are stored as entered
/// and are uppercase by convention (CH_BFS, CH_MIN). Bucketing on lowercased keys therefore matches no
/// agent row, every bucket is dropped, and the facet comes back <b>empty</b> — with no error anywhere.
/// </para>
/// <para>
/// The term-matching side must keep using the lowercased field, because the authorization clause and
/// the publishers filter both lowercase their input. So the two concerns need two fields, exactly as
/// the previous engine did it (a case-preserving FacetField beside a lowercased string field). These
/// tests fail if either half is pointed at the wrong field.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class PublisherFacetCasingTests
{
    private const string TermField = "publisherIdentifier";
    private const string FacetField = "publisherIdentifierLabel";

    private static string CountBodyJson(CatalogSearchFilter? filter = null) =>
        JsonSerializer.Serialize(CatalogQueryBuilder.BuildCountBody(
            queryString: null,
            languages: ["de"],
            filter: filter,
            role: BusinessRole.SwissDataSteward,
            agencies: []));

    [Test]
    public void Publishers_facet_buckets_on_the_case_preserving_field()
    {
        var aggregations = JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildCountBody(null, ["de"], null, BusinessRole.SwissDataSteward, [])["aggs"]);

        aggregations.Should().Contain($"\"field\":\"{FacetField}\"",
            because: "the publishers facet must bucket on the case-preserving field or every bucket is dropped by the agent lookup");
    }

    [Test]
    public void Publishers_filter_still_matches_on_the_lowercased_field()
    {
        var filter = new CatalogSearchFilter { PublisherIdentifiers = ["CH_BFS"] };

        var body = CountBodyJson(filter);

        // Lowercased both in the field chosen and in the value, so a filter arriving in any casing
        // still matches the indexed token.
        body.Should().Contain($"\"{TermField}\":[\"ch_bfs\"]");
        body.Should().NotContain($"\"{TermField}\":[\"CH_BFS\"]");
    }

    [Test]
    public void Authorization_clause_still_matches_on_the_lowercased_field()
    {
        var query = JsonSerializer.Serialize(CatalogQueryBuilder.BuildSearchBody(
            queryString: null,
            languages: ["de"],
            filter: null,
            role: BusinessRole.LocalDataSteward,
            agencies: ["CH_BFS"],
            from: 0,
            size: 10)["query"]);

        query.Should().Contain($"\"{TermField}\":[\"ch_bfs\"]");
        query.Should().NotContain(FacetField,
            because: "the facet field is not analyzed for authorization and must never appear in the auth clause");
    }

    /// <summary>
    /// The other half of the contract: the query above is useless if the document stops carrying the
    /// case-preserving field, and that failure is invisible — Elasticsearch happily aggregates a
    /// missing field and returns zero buckets.
    /// </summary>
    [Test]
    public void Indexed_document_carries_both_casings()
    {
        var system = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch };

        var document = CatalogDocumentFactory.Build(new CatalogIndexEntry
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.Concept,
            Identifier = "concept-1",
            Name = new MultiLanguageModel { De = "Bevoelkerung" },
            Title = new MultiLanguageModel { De = "Bevoelkerung" },
            Description = new MultiLanguageModel { De = "Beschreibung" },
            Version = "1.0.0",
            PublisherId = Guid.NewGuid(),
            PublisherIdentifier = "CH_BFS",
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            CreatedAt = DateTimeOffset.UnixEpoch,
            ConceptType = ConceptType.CodeList,
        }).Document;

        document[TermField].Should().Be("ch_bfs");
        document[FacetField].Should().Be("CH_BFS");
    }
}
