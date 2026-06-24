using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Core.UnitTests.Helpers;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Authorization;

[TestFixture(TestOf = typeof(EntityAuthorizationService))]
internal sealed class EntityAuthorizationServiceTests
{
    private readonly IEnumerable<BusinessRole> _allowedBusinessRoles =
        [
        BusinessRole.Submitter,
        BusinessRole.LocalDataSteward,
        BusinessRole.InteroperabilityService,
        BusinessRole.SwissDataSteward
        ];

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, true)]
    public void Given_user_with_BusinessRole_When_EnsureUserHasRoleToCreateEntities_Then_expected(
        string userBusinessRole,
        bool isAuthorizedExpected)
    {
        // Arrange
        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole)
        };

        var subject = new EntityAuthorizationService(TestHelper.CreateFakeUserContextService(claims));

        // Act
        var action = () => subject.EnsureUserHasRoleToCreateEntities(_allowedBusinessRoles);

        // Assert
        if (isAuthorizedExpected)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, true)]
    public void Given_user_with_BusinessRole_When_EnsureUserHasRoleToUpdateEntities_Then_expected(
        string userBusinessRole,
        bool isAuthorizedExpected)
    {
        // Arrange
        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole)
        };

        var subject = new EntityAuthorizationService(TestHelper.CreateFakeUserContextService(claims));

        // Act
        var action = () => subject.EnsureUserHasRoleToUpdateEntities(_allowedBusinessRoles);

        // Assert
        if (isAuthorizedExpected)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, true)]
    public void Given_user_with_BusinessRole_When_EnsureUserHasRoleToDeleteEntities_Then_expected(
        string userBusinessRole,
        bool isAuthorizedExpected)
    {
        // Arrange
        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole)
        };

        var subject = new EntityAuthorizationService(TestHelper.CreateFakeUserContextService(claims));

        // Act
        var action = () => subject.EnsureUserHasRoleToDeleteEntities(_allowedBusinessRoles);

        // Assert
        if (isAuthorizedExpected)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }
}
