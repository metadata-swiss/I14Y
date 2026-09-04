using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Authorization;
using Bfs.Iop.DataAccess.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Helpers;
using System.Security.Claims;

namespace Bfs.Iop.DataAccess.UnitTests.Authorization;

[TestFixture(TestOf = typeof(RegistrationStatusPolicyService))]
internal sealed class RegistrationStatusPolicyServiceTests
{
    [TestCase((RegistrationStatus)0, null, "toto")]
    [TestCase(RegistrationStatus.Candidate, (RegistrationStatus)0, "toto")]
    [TestCase(RegistrationStatus.Candidate, null, null!)]
    [TestCase(RegistrationStatus.Candidate, null, "")]
    [TestCase(RegistrationStatus.Candidate, null, " ")]
    public void Given_invalid_arguments_When_GetUserAllowedProposals_Then_throw_exception(
        RegistrationStatus currentStatus,
        RegistrationStatus? currentProposal,
        string agencyIdentifier)
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var subject = new RegistrationStatusPolicyService(userContextService);

        // Act
        var action = () => subject.GetUserAllowedProposals(currentStatus, currentProposal, agencyIdentifier, out _);

        // Assert
        action.Should().Throw<Exception>();
    }

    [TestCaseSource(nameof(GetUserAllowedProposalsTestCases))]
    public void Given_arguments_When_GetUserAllowedProposals_Then_return_expected(
        BusinessRole userBusinessRole,
        string userAgency,
        RegistrationStatus currentStatus,
        RegistrationStatus? currentProposal,
        string agencyIdentifier,
        IEnumerable<RegistrationStatus> expectedValues,
        bool expectedCanUserRevertProposal)
    {
        // Arrange      
        var claims = new[]
        {
            new Claim(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency),
            new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, GetBusinessRoleClaimString(userBusinessRole)),
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = new RegistrationStatusPolicyService(userContextService);

        // Act
        var result = subject.GetUserAllowedProposals(currentStatus, currentProposal, agencyIdentifier, out var userCanRevertProposal);

        // Assert
        using var _ = new AssertionScope();
        result.Should().BeEquivalentTo(expectedValues);
        userCanRevertProposal.Should().Be(expectedCanUserRevertProposal);
    }

    [TestCase((RegistrationStatus)0, PublicationLevel.Internal, "toto")]
    [TestCase(RegistrationStatus.Candidate, (PublicationLevel)0, "toto")]
    [TestCase(RegistrationStatus.Candidate, PublicationLevel.Internal, null!)]
    [TestCase(RegistrationStatus.Candidate, PublicationLevel.Internal, "")]
    [TestCase(RegistrationStatus.Candidate, PublicationLevel.Internal, " ")]
    public void Given_invalid_arguments_When_GetUserAllowedValidations_Then_throw_exception(
        RegistrationStatus currentStatus,
        PublicationLevel currentPublicationLevel,
        string agencyIdentifier)
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var subject = new RegistrationStatusPolicyService(userContextService);

        // Act
        var action = () => subject.GetUserAllowedValidations(currentStatus, currentPublicationLevel, agencyIdentifier);

        // Assert
        action.Should().Throw<Exception>();
    }

    [TestCaseSource(nameof(GetUserAllowedValidationTestCases))]
    public void Given_arguments_When_GetUserAllowedValidations_Then_return_expected(
        BusinessRole userBusinessRole,
        string userAgency,
        RegistrationStatus currentStatus,
        PublicationLevel currentPublicationLevel,
        string agencyIdentifier,
        IEnumerable<RegistrationStatus> expected)
    {
        // Arrange      
        var claims = new[]
        {
            new Claim(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency),
            new Claim(IopClaimsHelper.ClaimTypes.RoleClaimType, GetBusinessRoleClaimString(userBusinessRole)),
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = new RegistrationStatusPolicyService(userContextService);

        // Act
        var result = subject.GetUserAllowedValidations(currentStatus, currentPublicationLevel, agencyIdentifier);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    private static IEnumerable<TestCaseData> GetUserAllowedProposalsTestCases()
    {
        var allStatus = Enum.GetValues<RegistrationStatus>();

        yield return new TestCaseData(
            BusinessRole.StewardshipOrganisationViewer,
            "toto",
            RegistrationStatus.Incomplete,
            null,
            "toto",
            Enumerable.Empty<RegistrationStatus>(),
            false).SetArgDisplayNames("Test 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            RegistrationStatus.Incomplete,
            null,
            "toto",
            allStatus.Except([RegistrationStatus.Incomplete]),
            false).SetArgDisplayNames("Test 2");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "tata",
            RegistrationStatus.Incomplete,
            null,
            "toto",
            Enumerable.Empty<RegistrationStatus>(),
            false).SetArgDisplayNames("Test 3");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "toto",
            RegistrationStatus.Incomplete,
            null,
            "toto",
            allStatus.Except([RegistrationStatus.Incomplete]),
            false).SetArgDisplayNames("Test 4");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.Incomplete,
            null,
            "toto",
            Enumerable.Empty<RegistrationStatus>(),
            false).SetArgDisplayNames("Test 5");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "*",
            RegistrationStatus.Candidate,
            RegistrationStatus.Recorded,
            "toto",
            allStatus.Except([RegistrationStatus.Candidate, RegistrationStatus.Recorded]),
            true).SetArgDisplayNames("Test 6");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            RegistrationStatus.Recorded,
            null,
            "toto",
            allStatus.Except([RegistrationStatus.Recorded]),
            false).SetArgDisplayNames("Test 7");

        yield return new TestCaseData(
            BusinessRole.SwissDataSteward,
            "",
            RegistrationStatus.Recorded,
            RegistrationStatus.Incomplete,
            "toto",
            allStatus.Except([RegistrationStatus.Recorded, RegistrationStatus.Incomplete]),
            true).SetArgDisplayNames("Test 8");
    }

    private static IEnumerable<TestCaseData> GetUserAllowedValidationTestCases()
    {
        var allStatus = Enum.GetValues<RegistrationStatus>();

        yield return new TestCaseData(
            BusinessRole.StewardshipOrganisationViewer,
            "toto",
            RegistrationStatus.Incomplete,
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test StewardshipOrganisationViewer 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "toto",
            RegistrationStatus.Incomplete,
            PublicationLevel.Internal,
            "toto",
            new[] { RegistrationStatus.Candidate, RegistrationStatus.Superseded, RegistrationStatus.Retired }).SetArgDisplayNames("Test Submitter 1");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "tata",
            RegistrationStatus.Incomplete,
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test Submitter 2");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "tata",
            RegistrationStatus.Incomplete,
            PublicationLevel.Public,
            "tata",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test Submitter 3");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "tata",
            RegistrationStatus.Superseded,
            PublicationLevel.Internal,
            "tata",
            new[] { RegistrationStatus.Retired }).SetArgDisplayNames("Test Submitter 4");

        yield return new TestCaseData(
            BusinessRole.Submitter,
            "tata",
            RegistrationStatus.Recorded,
            PublicationLevel.Internal,
            "tata",
            new[] { RegistrationStatus.Superseded, RegistrationStatus.Retired }).SetArgDisplayNames("Test Submitter 5");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.Recorded,
            PublicationLevel.Internal,
            "tata",
            allStatus.Except([RegistrationStatus.Recorded, RegistrationStatus.Standard, RegistrationStatus.PreferredStandard])).SetArgDisplayNames("Test LocalDataSteward 1");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.Recorded,
            PublicationLevel.Internal,
            "toto",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test LocalDataSteward 2");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.Standard,
            PublicationLevel.Internal,
            "tata",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test LocalDataSteward 3");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.PreferredStandard,
            PublicationLevel.Internal,
            "tata",
            Enumerable.Empty<RegistrationStatus>()).SetArgDisplayNames("Test LocalDataSteward 4");

        yield return new TestCaseData(
            BusinessRole.LocalDataSteward,
            "tata",
            RegistrationStatus.Retired,
            PublicationLevel.Internal,
            "tata",
            new[] { RegistrationStatus.Superseded }).SetArgDisplayNames("Test LocalDataSteward 5");

        yield return new TestCaseData(
            BusinessRole.SwissDataSteward,
            "",
            RegistrationStatus.PreferredStandard,
            PublicationLevel.Internal,
            "tata",
            allStatus.Except([RegistrationStatus.PreferredStandard])).SetArgDisplayNames("Test SwissDataSteward 1");

        yield return new TestCaseData(
            BusinessRole.InteroperabilityService,
            "",
            RegistrationStatus.PreferredStandard,
            PublicationLevel.Internal,
            "tata",
            allStatus.Except([RegistrationStatus.PreferredStandard])).SetArgDisplayNames("Test InteroperabilityService 1");
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
