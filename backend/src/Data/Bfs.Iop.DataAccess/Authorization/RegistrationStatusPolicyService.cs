using Bfs.Iop.Common.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.DataAccess.Authorization;

internal sealed class RegistrationStatusPolicyService : IRegistrationStatusPolicyService
{
    private static readonly IEnumerable<RegistrationStatus> _allValues =
        Enum.GetValues<RegistrationStatus>();

    private readonly IUserContextService _userContextService;

    public RegistrationStatusPolicyService(IUserContextService userContextService) =>
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));

    public IEnumerable<RegistrationStatus> GetUserAllowedProposals(
        RegistrationStatus currentStatus,
        RegistrationStatus? currentProposal,
        string agencyIdentifier,
        out bool canUserRevertProposal)
    {
        currentStatus.EnsureValueIsValid();
        currentProposal?.EnsureValueIsValid();

        ArgumentException.ThrowIfNullOrWhiteSpace(agencyIdentifier, nameof(agencyIdentifier));

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        (var allowedProposals, canUserRevertProposal) = userBusinessRole switch
        {
            BusinessRole.InteroperabilityService
            or BusinessRole.SwissDataSteward => (_allValues, true),

            BusinessRole.LocalDataSteward
            or BusinessRole.Submitter => _userContextService.UserBelongsToAgency(agencyIdentifier)
                ? (_allValues, true)
                : ([], false),

            BusinessRole.StewardshipOrganisationViewer
            or BusinessRole.Unknown => ([], false),
            _ => throw new NotSupportedException($"The business role '{userBusinessRole}' is not supported.")
        };

        canUserRevertProposal &= currentProposal is not null;

        return allowedProposals.Except(
            currentProposal.HasValue
                ? [currentStatus, currentProposal!.Value]
                : [currentStatus]);
    }

    public IEnumerable<RegistrationStatus> GetUserAllowedValidations(
        RegistrationStatus currentStatus,
        PublicationLevel currentPublicationLevel,
        string agencyIdentifier)
    {
        currentStatus.EnsureValueIsValid();
        currentPublicationLevel.EnsureValueIsValid();

        ArgumentException.ThrowIfNullOrWhiteSpace(agencyIdentifier, nameof(agencyIdentifier));

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        var allowedValidations = userBusinessRole switch
        {
            BusinessRole.InteroperabilityService 
            or BusinessRole.SwissDataSteward => _allValues.Except([currentStatus]),

            BusinessRole.LocalDataSteward => _userContextService.UserBelongsToAgency(agencyIdentifier)
                ? GetLocalDataStewardAllowedValidations(currentStatus)
                : [],

            BusinessRole.Submitter => _userContextService.UserBelongsToAgency(agencyIdentifier) && 
                currentPublicationLevel is PublicationLevel.Internal
                    ? GetSubmitterAllowedValidations(currentStatus)
                    : [],

            BusinessRole.StewardshipOrganisationViewer
            or BusinessRole.Unknown => [],
            _ => throw new NotSupportedException($"The business role '{userBusinessRole}' is not supported.")
        };

        return allowedValidations;
    }

    private static IEnumerable<RegistrationStatus> GetLocalDataStewardAllowedValidations(
        RegistrationStatus currentStatus) => 
            currentStatus switch
            {
                RegistrationStatus.Incomplete
                or RegistrationStatus.Candidate
                or RegistrationStatus.Recorded
                or RegistrationStatus.Qualified => _allValues.Except(
                    [currentStatus, RegistrationStatus.Standard, RegistrationStatus.PreferredStandard]),

                RegistrationStatus.Standard
                or RegistrationStatus.PreferredStandard => [],

                RegistrationStatus.Superseded => [RegistrationStatus.Retired],
                RegistrationStatus.Retired => [RegistrationStatus.Superseded],
                _ => throw new NotSupportedException($"The registration status '{currentStatus}' is not supported.")
            };

    private static IEnumerable<RegistrationStatus> GetSubmitterAllowedValidations(
        RegistrationStatus currentStatus) =>
            currentStatus switch
            {
                RegistrationStatus.Incomplete
                or RegistrationStatus.Candidate => _allValues.Except(
                    [currentStatus, 
                    RegistrationStatus.Recorded,
                    RegistrationStatus.Qualified,
                    RegistrationStatus.Standard, 
                    RegistrationStatus.PreferredStandard]),
            
                RegistrationStatus.Recorded
                or RegistrationStatus.Qualified 
                or RegistrationStatus.Superseded
                or RegistrationStatus.Retired => (new[] { RegistrationStatus.Superseded, RegistrationStatus.Retired }).Except([currentStatus]),

                RegistrationStatus.Standard
                or RegistrationStatus.PreferredStandard => [],

                _ => throw new NotSupportedException($"The registration status '{currentStatus}' is not supported.")
            };
}
