using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Authorization;

[TestFixture(TestOf = typeof(PublishableEntityAuthorizationService))]
internal sealed class PublishableEntityAuthorizationServiceTests
{
    private static readonly IEnumerable<BusinessRole> _allowedBusinessRoles =
        [
        BusinessRole.Submitter, 
        BusinessRole.LocalDataSteward,
        BusinessRole.InteroperabilityService, 
        BusinessRole.SwissDataSteward
        ];

    [TestCaseSource(nameof(GetReadAuthorizationTests))]
    public void Given_user_role_and_entity_with_level_When_AppendUserReadAuthorizationConditionToDatabaseQuery_Then_expected(
        string userBusinessRole,
        bool userBelongsToEntityAgency,
        PublicationLevel entityLevel,
        bool isAuthorized)
    {
        // Arrange
        var entity = EntitiesHelper.IopConcept;
        entity.PublicationLevel = entityLevel;

        var userAgency = userBelongsToEntityAgency
            ? EntitiesHelper.Agent.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
{
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
};
        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(userContextService);

        using var dbContext = TestHelper.CreateFakeIopDbContext(userContextService);
        dbContext.IopConcepts.Add(entity);
        dbContext.SaveChanges();

        var query = dbContext.IopConcepts.AsQueryable();
        query = subject.AppendUserReadAuthorizationConditionToDatabaseQuery(query);

        // Act
        var result = query.Count();

        // Assert
        if (isAuthorized)
        {
            result.Should().Be(1);
        }
        else
        {
            result.Should().Be(0);
        }

        dbContext.Database.EnsureDeleted();
    }

    [TestCaseSource(nameof(GetReadAuthorizationTests))]
    public void Given_user_role_and_entity_When_EnsureUserCanReadPublishableEntity_Then_expected(
        string userBusinessRole,
        bool userBelongsToEntityAgency,
        PublicationLevel entityPublicationLevel,
        bool isAuthorized)
    {
        // Arrange
        var entity = EntitiesHelper.IopConcept;
        entity.Publisher = EntitiesHelper.Agent;

        entity.PublicationLevel = entityPublicationLevel;

        var userAgency = userBelongsToEntityAgency
            ? EntitiesHelper.Agent.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
{
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
};
        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(userContextService);

        // Act
        var action = () => subject.EnsureUserCanReadPublishableEntity(entity);

        // Assert
        if (isAuthorized)
        {
            action.Should().NotThrow();
        }
        else
        {
            if (userBusinessRole == "Unknown")
            {
                action.Should().ThrowExactly<UnauthorizedException>();
            }
            else
            {
                action.Should().ThrowExactly<ForbiddenException>();
            }
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, false, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, true)]
    public void Given_user_role_and_entity_When_EnsureUserCanCreateEntity_Then_expected(
        string userBusinessRole,
        bool userBelongsToEntityAgency,
        bool isAuthorized)
    {
        // Arrange
        var entity = EntitiesHelper.IopConcept;
        entity.Publisher = EntitiesHelper.Agent;

        var userAgency = userBelongsToEntityAgency
            ? EntitiesHelper.Agent.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
        };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(userContextService);

        // Act
        var action = () => subject.EnsureUserCanCreateEntity(entity.Publisher.Identifier, _allowedBusinessRoles);

        // Assert
        if (isAuthorized)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Public, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Standard, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, false, PublicationLevel.Internal, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Public, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Standard, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Internal, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Public, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Public, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Standard, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    public void Given_user_role_and_entity_with_level_and_status_When_EnsureUserCanUpdateEntity_Then_expected(
        string userBusinessRole,
        bool userBelongsToEntityAgency,
        PublicationLevel entityLevel,
        RegistrationStatus entityStatus,
        bool isAuthorized)
    {
        // Arrange
        var entity = EntitiesHelper.IopConcept;
        entity.Publisher = EntitiesHelper.Agent;
        entity.PublicationLevel = entityLevel;
        entity.RegistrationStatus = entityStatus;

        var userAgency = userBelongsToEntityAgency
            ? EntitiesHelper.Agent.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
{
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
};
        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(userContextService);

        // Act
        var action = () => subject.EnsureUserCanUpdateEntity(entity, _allowedBusinessRoles);

        // Assert
        if (isAuthorized)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Public, RegistrationStatus.Incomplete, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Standard, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, false, PublicationLevel.Internal, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Public, RegistrationStatus.Incomplete, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Standard, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Internal, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Public, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Public, RegistrationStatus.Incomplete, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Standard, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Incomplete, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, RegistrationStatus.Candidate, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, false, PublicationLevel.Internal, RegistrationStatus.Candidate, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, false, PublicationLevel.Public, RegistrationStatus.Candidate, false)]
    public void Given_user_role_and_entity_with_level_and_status_When_EnsureUserCanDeleteEntity_Then_expected(
        string userBusinessRole,
        bool userBelongsToEntityAgency,
        PublicationLevel entityLevel,
        RegistrationStatus entityStatus,
        bool isAuthorized)
    {
        // Arrange
        var entity = EntitiesHelper.IopConcept;
        entity.Publisher = EntitiesHelper.Agent;
        entity.PublicationLevel = entityLevel;
        entity.RegistrationStatus = entityStatus;

        var userAgency = userBelongsToEntityAgency
            ? EntitiesHelper.Agent.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
{
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
};
        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(userContextService);

        // Act
        var action = () => subject.EnsureUserCanDeleteEntity(entity, _allowedBusinessRoles);

        // Assert
        if (isAuthorized)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().Throw<Exception>().Which.Should().BeAssignableTo(typeof(IAllowActionInfoException));
        }
    }

    private static IEnumerable<TestCaseData> GetReadAuthorizationTests()
    {
        yield return new TestCaseData("Unknown", false, PublicationLevel.Internal, false).SetArgDisplayNames("TestCase1");
        yield return new TestCaseData("Unknown", false, PublicationLevel.Public, true).SetArgDisplayNames("TestCase2");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false, PublicationLevel.Internal, false).SetArgDisplayNames("TestCase3");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, true, PublicationLevel.Internal, true).SetArgDisplayNames("TestCase4");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false, PublicationLevel.Public, true).SetArgDisplayNames("TestCase5");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, false, PublicationLevel.Internal, false).SetArgDisplayNames("TestCase6");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, true, PublicationLevel.Internal, true).SetArgDisplayNames("TestCase7");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.Submitter, false, PublicationLevel.Public, true).SetArgDisplayNames("TestCase8");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.Submitter, false, PublicationLevel.Internal, false).SetArgDisplayNames("TestCase9");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, PublicationLevel.Internal, true).SetArgDisplayNames("TestCase10");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Public, true).SetArgDisplayNames("TestCase11");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, PublicationLevel.Internal, false).SetArgDisplayNames("TestCase12");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, PublicationLevel.Internal, true).SetArgDisplayNames("TestCase13");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Public, true).SetArgDisplayNames("TestCase14");
        yield return new TestCaseData(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, PublicationLevel.Internal, true).SetArgDisplayNames("TestCase15");
    }


    private static PublishableEntityAuthorizationService CreateFakeService(
        IUserContextService userContextService) =>
        new(
            userContextService,
            Substitute.For<IPublicationLevelPolicyService>(),
            Substitute.For<IRegistrationStatusPolicyService>());
}
