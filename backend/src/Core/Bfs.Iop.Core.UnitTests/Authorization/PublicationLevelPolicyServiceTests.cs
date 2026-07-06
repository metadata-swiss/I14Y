using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Authorization;

[TestFixture(TestOf = typeof(PublicationLevelPolicyService))]
internal sealed class PublicationLevelPolicyServiceTests
{
    [TestCase((PublicationLevel)0, null, "toto")]
    [TestCase(PublicationLevel.Public, (PublicationLevel)0, "toto")]
    [TestCase(PublicationLevel.Public, null, null)]
    [TestCase(PublicationLevel.Public, PublicationLevel.Internal, "")]
    [TestCase(PublicationLevel.Public, null, " ")]
    public void Given_invalid_arguments_When_GetUserAllowedProposals_Then_throw_exception(
        PublicationLevel currentLevel,
        PublicationLevel? currentProposal,
        string agencyIdentifier)
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var subject = new PublicationLevelPolicyService(userContextService);

        // Act
        var action = () => subject.GetUserAllowedProposals(currentLevel, currentProposal, agencyIdentifier, out _);

        // Assert
        action.Should().Throw<Exception>();
    }

    [TestCaseSource(nameof(GetUserAllowedProposalsTestCases))]
    public void Given_arguments_When_GetUserAllowedProposals_Then_return_expected(
        BusinessRole userBusinessRole,
        string userAgency,
        PublicationLevel currentLevel,
        PublicationLevel? currentProposal,
        string agencyIdentifier,
        IEnumerable<PublicationLevel> expected,
        bool expectedUserCanRevertProposal)
    {
        // Arrange      
        var claims = new[]
        {
            new Claim(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency),
            new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, GetBusinessRoleClaimString(userBusinessRole)),
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = new PublicationLevelPolicyService(userContextService);

        // Act
        var result = subject.GetUserAllowedProposals(currentLevel, currentProposal, agencyIdentifier, out var canUserRevertProposal);

        // Assert
        using var _ = new AssertionScope();
        result.Should().BeEquivalentTo(expected);
        canUserRevertProposal.Should().Be(expectedUserCanRevertProposal);
    }

    [TestCase((PublicationLevel)0, "toto")]
    [TestCase(PublicationLevel.Public, null)]
    [TestCase(PublicationLevel.Public, "")]
    [TestCase(PublicationLevel.Public, " ")]
    public void Given_invalid_arguments_When_GetUserAllowedValidations_Then_throw_exception(
    PublicationLevel currentLevel,
    string agencyIdentifier)
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var subject = new PublicationLevelPolicyService(userContextService);

        // Act
        var action = () => subject.GetUserAllowedValidations(currentLevel, agencyIdentifier);

        // Assert
        action.Should().Throw<Exception>();
    }

    [TestCaseSource(nameof(GetUserAllowedValidationsTestCases))]
    public void Given_arguments_When_GetUserAllowedValidations_Then_return_expected(
        BusinessRole userBusinessRole,
        string userAgency,
        PublicationLevel currentLevel,
        string agencyIdentifier,
        IEnumerable<PublicationLevel> expected)
    {
        // Arrange
        var claims = new[]
{
            new Claim(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency),
            new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, GetBusinessRoleClaimString(userBusinessRole)),
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = new PublicationLevelPolicyService(userContextService);

        // Act
        var result = subject.GetUserAllowedValidations(currentLevel, agencyIdentifier);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    private static IEnumerable<TestCaseData> GetUserAllowedProposalsTestCases()
    {
        var allLevels = Enum.GetValues<PublicationLevel>();

        yield return new TestCaseData(
            BusinessRole.StewardshipOrganisationViewer,
            "toto",
            PublicationLevel.Internal,
            null,
            "toto",
            Enumerable.Empty<PublicationLevel>(),
            false).SetArgDisplayNames("Test StewardshipOrganisationViewer 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            PublicationLevel.Internal,
            null,
            "toto",
            allLevels.Except([PublicationLevel.Internal]),
            false).SetArgDisplayNames("Test Submitter 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            PublicationLevel.Public,
            null,
            "toto",
            allLevels.Except([PublicationLevel.Public]),
            false).SetArgDisplayNames("Test Submitter 2");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            PublicationLevel.Public,
            null,
            "tata",
            Enumerable.Empty<PublicationLevel>(),
            false).SetArgDisplayNames("Test Submitter 3");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            PublicationLevel.Public,
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<PublicationLevel>(),
            true).SetArgDisplayNames("Test Submitter 4");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "toto",
            PublicationLevel.Public,
            null,
             "toto",
            allLevels.Except([PublicationLevel.Public]),
            false).SetArgDisplayNames("Test LocalDataSteward 1");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "toto",
            PublicationLevel.Public,
            null,
            "tata",
            Enumerable.Empty<PublicationLevel>(),
            false).SetArgDisplayNames("Test LocalDataSteward 2");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            PublicationLevel.Public,
            null,
            "tata",
            allLevels.Except([PublicationLevel.Public]),
            false).SetArgDisplayNames("Test InteroperabilityService 1");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            PublicationLevel.Public,
            PublicationLevel.Internal,
            "tata",
            Enumerable.Empty<PublicationLevel>(),
            true).SetArgDisplayNames("Test InteroperabilityService 2");

        yield return new TestCaseData(
            BusinessRole.SwissDataSteward,
            "",
            PublicationLevel.Public,
            null,
            "tata",
            allLevels.Except([PublicationLevel.Public]),
            false).SetArgDisplayNames("Test SwissDataSteward 1");

        yield return new TestCaseData(
            BusinessRole.SwissDataSteward,
            "",
            PublicationLevel.Public,
            PublicationLevel.Internal,
            "tata",
            Enumerable.Empty<PublicationLevel>(),
            true).SetArgDisplayNames("Test SwissDataSteward 2");
    }

    private static IEnumerable<TestCaseData> GetUserAllowedValidationsTestCases()
    {
        var allLevels = Enum.GetValues<PublicationLevel>();

        yield return new TestCaseData(
            BusinessRole.StewardshipOrganisationViewer,
            "toto",
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<PublicationLevel>()).SetArgDisplayNames("Test StewardshipOrganisationViewer 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<PublicationLevel>()).SetArgDisplayNames("Test Submitter 1");
        
        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "toto",
            PublicationLevel.Internal,
            "toto",
            allLevels.Except([PublicationLevel.Internal])).SetArgDisplayNames("Test LocalDataSteward 1");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "toto",
            PublicationLevel.Internal,
            "tata",
            Enumerable.Empty<PublicationLevel>()).SetArgDisplayNames("Test LocalDataSteward 2");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            PublicationLevel.Internal,
            "toto",
            allLevels.Except([PublicationLevel.Internal])).SetArgDisplayNames("Test InteroperabilityService 1");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            PublicationLevel.Public,
            "toto",
            allLevels.Except([PublicationLevel.Public])).SetArgDisplayNames("Test InteroperabilityService 2");

        yield return new TestCaseData(
            BusinessRole.SwissDataSteward,
            "",
            PublicationLevel.Internal,
            "toto",
            allLevels.Except([PublicationLevel.Internal])).SetArgDisplayNames("Test SwissDataSteward 1");
    }

    private static string GetBusinessRoleClaimString(BusinessRole businessRole) =>
        businessRole switch
        {
            BusinessRole.StewardshipOrganisationViewer => IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer,
            BusinessRole.LocalDataSteward => IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward,
            BusinessRole.InteroperabilityService => IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService,
            BusinessRole.SwissDataSteward => IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward,
            BusinessRole.Submitter => IopClaimsHelper.Roles.BusinessRoles.Submitter,
            _ => throw new NotSupportedException($"The business role '{businessRole}' is not supported.")
        };
}
