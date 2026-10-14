using AwesomeAssertions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Contracts.Search;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// This is what decides how much of the index a caller sees, so a wrong answer here either leaks
// unpublished resources or hides a user's own work from them. Both are silent.
//
// Turning the token's claims into a role is IUserContextService's job and is tested with it. All
// this class does is refuse to trust an invalid token and carry the rest through untouched, so that
// is all these tests assert. Feeding claim strings to a substitute here only tested the substitute.
[TestFixture]
internal sealed class SearchCallerFactoryTests
{
    [Test]
    public void A_caller_without_a_valid_token_is_the_public_one()
    { 
        var caller = Create(valid: false, role: BusinessRole.InteroperabilityService, agencies: ["CH1"]);

        caller.Role.Should().Be(BusinessRole.Unknown);
        caller.Agencies.Should().BeEmpty();
    }

    [Test]
    public void The_role_the_user_context_resolved_is_the_one_the_caller_carries()
    { 
        Create(valid: true, role: BusinessRole.InteroperabilityService)
            .Role.Should().Be(BusinessRole.InteroperabilityService);
    }

    [Test]
    public void The_agencies_on_the_token_are_carried_through()
    { 
        var caller = Create(valid: true, role: BusinessRole.LocalDataSteward, agencies: ["CH1", "CH2"]);

        caller.Agencies.Should().Equal("CH1", "CH2");
    }

    private static SearchCaller Create(
        bool valid,
        BusinessRole role,
        IReadOnlyList<string>? agencies = null)
    {
        var userContext = Substitute.For<IUserContextService>();

        userContext.IsUserTokenValid().Returns(valid);
        userContext.GetUserBusinessRole().Returns(role);
        userContext.GetUserAgencies().Returns(agencies ?? []);

        return new SearchCallerFactory(userContext).Create();
    }
}
