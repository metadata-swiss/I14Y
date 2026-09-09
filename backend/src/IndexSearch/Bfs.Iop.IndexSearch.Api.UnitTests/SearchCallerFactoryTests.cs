using AwesomeAssertions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Contracts;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// This is what decides how much of the index a caller sees, so a wrong answer here either leaks
// unpublished resources or hides a user's own work from them. Both are silent.
[TestFixture]
internal sealed class SearchCallerFactoryTests
{
    private const string InteroperabilityService = IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService;
    private const string SwissDataSteward = IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward;
    private const string LocalDataSteward = IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward;
    private const string Submitter = IopClaimsHelper.Roles.BusinessRoles.Submitter;
    private const string StewardshipViewer = IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer;

    [Test]
    public void A_caller_without_a_valid_token_is_the_public_one()
    {
        var caller = Create(valid: false, roles: [InteroperabilityService]);

        // Even a token carrying the most privileged role must count for nothing if it did not validate.
        caller.Role.Should().Be(BusinessRole.Unknown);
        caller.Agencies.Should().BeEmpty();
    }

    [Test]
    public void A_valid_token_with_no_business_role_sees_what_the_public_sees()
    {
        // Unknown is what the authorization clause falls through to, and it filters to public only.
        Create(valid: true, roles: []).Role.Should().Be(BusinessRole.Unknown);
    }

    [TestCase(InteroperabilityService, BusinessRole.InteroperabilityService)]
    [TestCase(SwissDataSteward, BusinessRole.SwissDataSteward)]
    [TestCase(LocalDataSteward, BusinessRole.LocalDataSteward)]
    [TestCase(Submitter, BusinessRole.Submitter)]
    [TestCase(StewardshipViewer, BusinessRole.StewardshipOrganisationViewer)]
    public void Every_business_role_maps_to_its_own_index_role(string claim, BusinessRole expected)
    {
        // The last pair is the one to watch: the claim spells it "Organization" and the contract
        // "Organisation", so a careless rename drops the role to Unknown and quietly narrows the user
        // to public results.
        Create(valid: true, roles: [claim]).Role.Should().Be(expected);
    }

    [TestCase(Submitter, InteroperabilityService, BusinessRole.InteroperabilityService)]
    [TestCase(LocalDataSteward, SwissDataSteward, BusinessRole.SwissDataSteward)]
    [TestCase(StewardshipViewer, SwissDataSteward, BusinessRole.SwissDataSteward)]
    public void The_most_privileged_role_a_caller_holds_is_the_one_that_counts(
        string narrow,
        string wide,
        BusinessRole expected)
    {
        // A user can hold several roles. Resolving in the wrong order scopes a Swiss data steward to
        // their own agency, which hides resources they are entitled to see.
        Create(valid: true, roles: [narrow, wide]).Role.Should().Be(expected);
    }

    [Test]
    public void The_agencies_on_the_token_are_carried_through()
    {
        // The agency-scoped clause filters on these, so dropping them would leave a local data steward
        // seeing only public resources.
        var caller = Create(valid: true, roles: [LocalDataSteward], agencies: ["CH1", "CH2"]);

        caller.Agencies.Should().Equal("CH1", "CH2");
    }

    private static Contracts.Search.SearchCaller Create(
        bool valid,
        IReadOnlyList<string> roles,
        IReadOnlyList<string>? agencies = null)
    {
        var userContext = Substitute.For<IUserContextService>();

        userContext.IsUserTokenValid().Returns(valid);
        userContext.GetUserAgencies().Returns(agencies ?? []);

        foreach (var role in roles)
        {
            userContext.UserHasRole(role).Returns(true);
        }

        return new SearchCallerFactory(userContext).Create();
    }
}
