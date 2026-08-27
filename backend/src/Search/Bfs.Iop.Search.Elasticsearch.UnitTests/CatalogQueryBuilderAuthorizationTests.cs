using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Search.Elasticsearch.UnitTests;

/// <summary>
/// The authorization clause is what makes it safe to serve search from an index that deliberately
/// contains non-public resources.
/// <para>
/// Its failure mode is silent: get it wrong in one direction and entitled users lose results they
/// should see (an HTTP 200 that looks like missing data); wrong in the other and a caller is served
/// resources they are not allowed to see. Neither throws, so nothing but a test catches it. These
/// assertions run against the serialised query body — the thing Elasticsearch actually receives —
/// rather than any intermediate object.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogQueryBuilderAuthorizationTests
{
    private const string PublicationLevelField = "publicationLevel";
    private const string PublisherIdentifierField = "publisherIdentifier";

    /// <summary>Roles that see everything, so no authorization clause may be emitted at all.</summary>
    private static readonly BusinessRole[] UnrestrictedRoles =
    [
        BusinessRole.InteroperabilityService,
        BusinessRole.SwissDataSteward,
    ];

    /// <summary>Roles restricted to public resources plus their own agencies'.</summary>
    private static readonly BusinessRole[] RestrictedRoles =
    [
        BusinessRole.Unknown,
        BusinessRole.LocalDataSteward,
        BusinessRole.Submitter,
        BusinessRole.StewardshipOrganisationViewer,
    ];

    /// <summary>
    /// Serialises only the <c>query</c> node, not the whole body.
    /// <para>
    /// Necessary rather than tidy: the count body aggregates *on* <c>publicationLevel</c>, so a
    /// whole-body string match cannot tell an authorization filter from an aggregation target. The
    /// first version of this fixture asserted against the whole body and produced a false failure —
    /// which means it could equally have produced a false pass.
    /// </para>
    /// </summary>
    private static string QueryOf(Dictionary<string, object?> body) =>
        JsonSerializer.Serialize(body["query"]);

    private static string SearchQueryJson(BusinessRole role, params string[] agencies) =>
        QueryOf(CatalogQueryBuilder.BuildSearchBody(
            queryString: "bev",
            languages: ["de"],
            filter: null,
            role,
            agencies,
            from: 0,
            size: 10));

    private static string CountQueryJson(BusinessRole role, params string[] agencies) =>
        QueryOf(CatalogQueryBuilder.BuildCountBody(
            queryString: "bev",
            languages: ["de"],
            filter: null,
            role,
            agencies));

    [TestCaseSource(nameof(UnrestrictedRoles))]
    public void Unrestricted_roles_get_no_authorization_clause(BusinessRole role)
    {
        var json = SearchQueryJson(role, "bfs", "seco");

        // Not merely "no publisher list" — the publication-level term must be absent too, otherwise
        // these roles would silently lose every non-public resource.
        json.Should().NotContain(PublicationLevelField);
        json.Should().NotContain(PublisherIdentifierField);
    }

    [TestCaseSource(nameof(RestrictedRoles))]
    public void Restricted_roles_always_get_the_public_publication_level_clause(BusinessRole role)
    {
        var json = SearchQueryJson(role);

        json.Should().Contain(PublicationLevelField);
    }

    [TestCaseSource(nameof(RestrictedRoles))]
    public void The_publication_level_clause_pins_Public_and_not_merely_some_level(BusinessRole role)
    {
        var json = SearchQueryJson(role);

        // Asserting the field name alone is not enough, and this is the gap worth closing: if the
        // clause were changed to Internal, or the enum's numeric values were reordered, a field-only
        // assertion still passes while every restricted caller silently gains access to internal
        // resources. PublicationLevel is { Internal = 1, Public = 2 } and the clause emits the int.
        json.Should().Contain($"\"{PublicationLevelField}\":{(int)PublicationLevel.Public}");
        json.Should().NotContain($"\"{PublicationLevelField}\":{(int)PublicationLevel.Internal}");
    }

    [TestCaseSource(nameof(RestrictedRoles))]
    public void Restricted_roles_with_agencies_get_both_the_public_clause_and_their_agencies(BusinessRole role)
    {
        var json = SearchQueryJson(role, "bfs", "seco");

        json.Should().Contain(PublicationLevelField);
        json.Should().Contain(PublisherIdentifierField);
        json.Should().Contain("bfs").And.Contain("seco");

        // minimum_should_match = 1 makes it public OR mine. Without it the bool.should would be
        // advisory and the clause would filter nothing at all.
        json.Should().Contain("minimum_should_match");
    }

    [Test]
    public void An_anonymous_caller_is_restricted_to_public_resources_only()
    {
        // No role and no agencies is the anonymous case: Unknown is the default enum value, so a
        // caller whose token failed validation lands here. It must be the most restrictive branch.
        var json = SearchQueryJson(BusinessRole.Unknown);

        json.Should().Contain(PublicationLevelField);
        json.Should().NotContain(PublisherIdentifierField);
    }

    [Test]
    public void Agency_identifiers_are_lowercased_to_match_the_indexed_value()
    {
        var json = SearchQueryJson(BusinessRole.LocalDataSteward, "BFS", "SeCo");

        // The document factory lowercases publisherIdentifier, so a mixed-case claim would match
        // nothing and quietly reduce the caller to public-only results.
        json.Should().Contain("bfs").And.Contain("seco");
        json.Should().NotContain("BFS").And.NotContain("SeCo");
    }

    [TestCaseSource(nameof(RestrictedRoles))]
    public void The_count_query_carries_the_same_authorization_clause_as_the_search_query(BusinessRole role)
    {
        var count = CountQueryJson(role, "bfs");

        // Facet counts must be scoped exactly like results. If the count body were unauthorized, the
        // tabs would advertise resources the caller cannot open.
        count.Should().Contain(PublicationLevelField);
        count.Should().Contain(PublisherIdentifierField);
        count.Should().Contain("bfs");
    }

    [TestCaseSource(nameof(UnrestrictedRoles))]
    public void The_count_query_omits_the_clause_for_unrestricted_roles(BusinessRole role)
    {
        var count = CountQueryJson(role, "bfs");

        count.Should().NotContain(PublicationLevelField);
        count.Should().NotContain(PublisherIdentifierField);
    }
}
