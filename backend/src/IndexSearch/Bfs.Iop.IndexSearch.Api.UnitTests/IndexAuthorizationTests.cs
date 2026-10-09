using System.Security.Claims;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;
 
[TestFixture]
internal sealed class IndexAuthorizationTests
{
    private const string RoleClaim = IopClaimsHelper.ClaimTypes.RoleClaimType;
    private const string Service = IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService;

    [Test]
    public async Task The_interoperability_service_may_rebuild()
    {
        (await IsAllowed(Authenticated(new Claim(RoleClaim, Service)))).Should().BeTrue();
    }
 
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, TestName = "a Swiss data steward")]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, TestName = "a local data steward")]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, TestName = "a submitter")]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, TestName = "a stewardship viewer")]
    [TestCase(IopClaimsHelper.Roles.General.Allow, TestName = "the bare ALLOW role")]
    public async Task No_other_role_may_rebuild(string role)
    {
        (await IsAllowed(Authenticated(new Claim(RoleClaim, role)))).Should().BeFalse();
    }

    [Test]
    public async Task The_roles_a_real_steward_token_carries_are_not_enough()
    {
        // The shape of an actual eIAM steward token: ALLOW plus an agency-scoped role.
        var user = Authenticated(
            new Claim(RoleClaim, IopClaimsHelper.Roles.General.Allow),
            new Claim(RoleClaim, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward));

        (await IsAllowed(user)).Should().BeFalse();
    }

    [Test]
    public async Task The_service_role_among_others_is_enough()
    {
        var user = Authenticated(
            new Claim(RoleClaim, IopClaimsHelper.Roles.General.Allow),
            new Claim(RoleClaim, Service));

        (await IsAllowed(user)).Should().BeTrue();
    }

    [Test]
    public async Task A_caller_with_no_roles_at_all_may_not()
    {
        (await IsAllowed(Authenticated())).Should().BeFalse();
    }

    [Test]
    public async Task An_anonymous_caller_may_not()
    {
        // Search answers anonymously; rebuilding must not.
        (await IsAllowed(new ClaimsPrincipal(new ClaimsIdentity()))).Should().BeFalse();
    }

    [Test]
    public async Task An_unauthenticated_principal_carrying_the_claim_is_not_enough()
    {
        // A claim is not a credential. With no authentication type nothing validated the token it came
        // from, and RequireClaim on its own would accept it.
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(RoleClaim, Service)]));

        (await IsAllowed(user)).Should().BeFalse();
    }

    [Test]
    public async Task The_role_carried_under_the_framework_claim_type_is_not_enough()
    {
        // The trap the policy exists for: a role mapped onto ClaimTypes.Role is not the shape these
        // tokens arrive in, and accepting it would mean the rule passed for the wrong reason.
        (await IsAllowed(Authenticated(new Claim(ClaimTypes.Role, Service)))).Should().BeFalse();
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Bearer"));

    private static async Task<bool> IsAllowed(ClaimsPrincipal user)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        // The policy the host registers, not a copy of it: a change in Program.cs has to reach here.
        services.AddAuthorizationBuilder()
            .AddPolicy(IndexPolicies.Rebuild, IndexPolicies.ConfigureRebuild);

        var authorization = services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();

        return (await authorization.AuthorizeAsync(user, resource: null, IndexPolicies.Rebuild)).Succeeded;
    }
}
