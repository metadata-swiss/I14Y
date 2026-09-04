using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Infrastructure.Security.UnitTests.Helpers;
using System.Security.Claims;

namespace Bfs.Iop.Infrastructure.Security.UnitTests.Services;

[TestFixture(TestOf = typeof(UserContextService))]
internal sealed class UserContextServiceTests
{
    [TestCase(null, "CH1", false)]
    [TestCase("CH1", "CH1", true)]
    [TestCase("CH1", "BFS", false)]
    [TestCase("*", "CH1", true)]
    public void Given_agencyIdentifier_When_UserBelongsToAgency_Then_return_expected(
        string? belongedAgency,
        string testAgencyIdentifier,
        bool expected)
    {
        // Arrange
        var claims = belongedAgency is null
            ? []
            : new[] 
            { 
                new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.General.Allow),
                new Claim(IopClaimsHelper.ClaimTypes.AgenciesClaimType, belongedAgency) 
            };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        // Act
        var result = userContextService.UserBelongsToAgency(testAgencyIdentifier);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("toto", IopClaimsHelper.ClaimTypes.RoleClaimType, "toto", true)]
    [TestCase("toto", "UnknownClaimType", "toto", false)]
    [TestCase("toto", IopClaimsHelper.ClaimTypes.RoleClaimType, "tata", false)]
    public void Given_role_When_UserHasRole_Then_return_expected(string userRole, string claimType, string testRole, bool expected)
    {
        // Arrange
        var claims = new[] { new Claim(claimType, userRole) };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        // Act
        var result = userContextService.UserHasRole(testRole);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, BusinessRole.Submitter)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, BusinessRole.LocalDataSteward)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, BusinessRole.InteroperabilityService)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, BusinessRole.StewardshipOrganisationViewer)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, BusinessRole.SwissDataSteward)]
    [TestCase("toto", BusinessRole.Unknown)]
    public void When_GetUserBusinessRole_Then_return_expected(string role, BusinessRole expected)
    {
        // Arrange
        var claims = new[] { new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, role) };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        // Act
        var result = userContextService.GetUserBusinessRole();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}
