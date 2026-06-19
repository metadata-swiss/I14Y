using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using AwesomeAssertions;
using System.Security.Claims;

namespace Bfs.Iop.Core.Common.UnitTests.Extensions;

[TestFixture(TestOf = typeof(UserContextExtensions))]
internal class UserContextExtensionsTests
{
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
        result.Should().Be(expected);
    }
}
