using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogQueryBuilderAuthorizationTests
{
    private static readonly string[] German = ["de"];

    private static string SearchQueryJson(SearchCaller caller) =>
        JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, from: 0, size: 10)["query"]);

    private static SearchCaller As(BusinessRole role, params string[] agencies) =>
        new() { Role = role, Agencies = agencies };

    [TestCase(BusinessRole.SwissDataSteward)]
    [TestCase(BusinessRole.InteroperabilityService)]
    public void Unrestricted_roles_get_no_authorization_clause(BusinessRole role)
    {
        SearchQueryJson(As(role)).Should().NotContain(EsCatalogFields.PublicationLevel);
    }

    [Test]
    public void Anonymous_sees_public_only()
    {
        var query = SearchQueryJson(SearchCaller.Anonymous);

        query.Should().Contain("\"publicationLevel\":\"Public\"");
        query.Should().NotContain("Internal");
    }

    [Test]
    public void An_unmapped_role_falls_back_to_public_only()
    {
        var query = SearchQueryJson(As((BusinessRole)999));

        query.Should().Contain("\"publicationLevel\":\"Public\"");
        query.Should().NotContain("Internal");
    }

    [TestCase(BusinessRole.LocalDataSteward)]
    [TestCase(BusinessRole.Submitter)]
    [TestCase(BusinessRole.StewardshipOrganisationViewer)]
    public void Agency_scoped_roles_see_public_plus_their_own_internal(BusinessRole role)
    {
        var query = SearchQueryJson(As(role, "CH_BFS"));

        query.Should().Contain("\"publicationLevel\":\"Public\"");
        query.Should().Contain("\"publicationLevel\":\"Internal\"");
        // Lowercased, because that is how the identifier is indexed for term matching.
        query.Should().Contain("\"publisherIdentifier\":[\"ch_bfs\"]");
    }

    [Test]
    public void An_agency_scoped_role_with_no_agencies_collapses_to_public_only()
    {
        var query = SearchQueryJson(As(BusinessRole.LocalDataSteward));

        query.Should().Contain("\"publicationLevel\":\"Public\"");
        query.Should().NotContain("Internal");
    }

    [Test]
    public void The_count_query_still_carries_authorization()
    {
        var query = JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildCountBody(null, German, null, SearchCaller.Anonymous)["query"]);

        query.Should().Contain("\"publicationLevel\":\"Public\"");
    }
}
