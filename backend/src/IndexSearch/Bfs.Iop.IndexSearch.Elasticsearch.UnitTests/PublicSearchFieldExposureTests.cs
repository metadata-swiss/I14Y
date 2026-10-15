using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

/// <summary>
///     Which fields a free text search is allowed to match on, by caller.
/// </summary>
/// <remarks>
///     The public API returns a dataset's contact points but not its responsible person, deputy or
///     data owner. Matching on those let an anonymous caller search a person's name and get back every
///     resource they are accountable for - information the API exposes no other way.
/// </remarks>
[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class PublicSearchFieldExposureTests
{
    private static readonly string[] German = ["de"];

    private static readonly SearchCaller Steward =
        new() { Role = BusinessRole.SwissDataSteward };

    [TestCase(EsCatalogFields.ResponsiblePersonName)]
    [TestCase(EsCatalogFields.ResponsibleDeputyName)]
    [TestCase(EsCatalogFields.DataOwner)]
    public void A_public_search_does_not_match_on_who_is_accountable(string field)
    {
        Body("santi", SearchCaller.Anonymous).Should().NotContain(field);
    }

    [TestCase(EsCatalogFields.ResponsiblePersonEmail)]
    [TestCase(EsCatalogFields.ResponsibleDeputyEmail)]
    public void Nor_on_their_address_when_the_query_is_one(string field)
    {
        // The email path matches exact terms, so without this an anonymous caller who knows someone's
        // address could still list everything they are responsible for.
        Body("santi@bfs.admin.ch", SearchCaller.Anonymous).Should().NotContain(field);
    }

    [TestCase(EsCatalogFields.ResponsiblePersonEmail)]
    [TestCase(EsCatalogFields.ResponsibleDeputyEmail)]
    public void Nor_when_the_address_does_not_look_like_one(string field)
    {
        // An intranet address has no dotted domain, so it fails the regex and takes the ordinary free
        // text path - which also matches every email field as an exact term.
        Body("santi@intranet", SearchCaller.Anonymous).Should().NotContain(field);
    }

    [Test]
    public void The_ordinary_path_still_matches_the_public_addresses()
    {
        var anonymous = Body("santi@intranet", SearchCaller.Anonymous);

        anonymous.Should().Contain(EsCatalogFields.ContactPointHasEmail);
        anonymous.Should().Contain(EsCatalogFields.ChannelEmail);
    }

    [TestCase(EsCatalogFields.ResponsiblePersonName)]
    [TestCase(EsCatalogFields.DataOwner)]
    public void A_steward_still_can(string field)
    {
        Body("santi", Steward).Should().Contain(field);
    }

    [Test]
    public void Contact_points_stay_searchable_for_everyone()
    {
        // They are part of the public payload, so searching them exposes nothing new.
        var anonymous = Body("santi", SearchCaller.Anonymous);

        anonymous.Should().Contain(EsCatalogFields.ContactPointFn);
        anonymous.Should().Contain(EsCatalogFields.ContactPointHasTelephone);
        Body("santi@bfs.admin.ch", SearchCaller.Anonymous).Should().Contain(EsCatalogFields.ContactPointHasEmail);
    }

    [Test]
    public void A_public_visitor_is_exactly_a_caller_restricted_to_public_resources()
    {
        // Two places decide what "public" means: the clause that limits which resources a caller
        // matches, and the filter that limits which fields are searched. Walked over every role so a
        // new one cannot be added to one and forgotten in the other.
        foreach (var role in Enum.GetValues<BusinessRole>())
        {
            var caller = new SearchCaller { Role = role };

            var restrictedToPublic = JsonSerializer
                .Serialize(CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, 0, 10))
                .Contains($"\"{EsCatalogFields.PublicationLevel}\":\"Public\"", StringComparison.Ordinal);

            CatalogQueryBuilder.IsPublicVisitor(caller).Should().Be(
                restrictedToPublic && !IsAgencyScoped(caller),
                "role {0} must be treated the same way by both",
                role);
        }
    }

    private static bool IsAgencyScoped(SearchCaller caller) =>
        JsonSerializer.Serialize(CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, 0, 10))
            .Contains("minimum_should_match", StringComparison.Ordinal);

    [Test]
    public void Everything_the_public_cannot_search_is_absent_from_the_public_payload()
    {
        // The list is the contract: each entry is a field the public API does not return.
        EsCatalogFields.NonPublicFields.Should().BeEquivalentTo(
        [
            EsCatalogFields.DataOwner,
            EsCatalogFields.ResponsiblePersonName,
            EsCatalogFields.ResponsibleDeputyName,
            EsCatalogFields.ResponsiblePersonEmail,
            EsCatalogFields.ResponsibleDeputyEmail,
        ]);
    }

    private static string Body(string queryString, SearchCaller caller) =>
        JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(queryString, German, null, caller, from: 0, size: 10));
}
