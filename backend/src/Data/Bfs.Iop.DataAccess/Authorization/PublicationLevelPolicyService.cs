using Bfs.Iop.Common.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.DataAccess.Authorization;

internal sealed class PublicationLevelPolicyService : IPublicationLevelPolicyService
{
    private static readonly IEnumerable<PublicationLevel> _allLevels = Enum.GetValues<PublicationLevel>();

    private readonly IUserContextService _userContextService;

    public PublicationLevelPolicyService(IUserContextService userContextService) => 
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));

    public IEnumerable<PublicationLevel> GetUserAllowedProposals(
        PublicationLevel currentLevel,
        PublicationLevel? currentProposal,
        string agencyIdentifier,
        out bool canUserRevertProposal)
    {
        currentLevel.EnsureValueIsValid();
        currentProposal?.EnsureValueIsValid();
        ArgumentException.ThrowIfNullOrWhiteSpace(agencyIdentifier, nameof(agencyIdentifier));

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        (var allowedLevels, canUserRevertProposal) = userBusinessRole switch
        {
            BusinessRole.InteroperabilityService
            or BusinessRole.SwissDataSteward => (_allLevels, true),

            BusinessRole.LocalDataSteward 
            or BusinessRole.Submitter => _userContextService.UserBelongsToAgency(agencyIdentifier)
                ? (_allLevels, true)
                : ([], false),

            BusinessRole.StewardshipOrganisationViewer or
            BusinessRole.Unknown => ([], false),
            _ => throw new NotSupportedException($"The business role '{userBusinessRole}' is not supported.")
        };

        canUserRevertProposal &= currentProposal is not null;

        return allowedLevels.Except(
            currentProposal is null 
                ? [currentLevel] 
                : [currentLevel, currentProposal.Value]);
    }

    public IEnumerable<PublicationLevel> GetUserAllowedValidations(
        PublicationLevel currentLevel,
        string agencyIdentifier)
    {
        currentLevel.EnsureValueIsValid();
        ArgumentException.ThrowIfNullOrWhiteSpace(agencyIdentifier, nameof(agencyIdentifier));

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        var allowedValidations = userBusinessRole switch
        {
            BusinessRole.InteroperabilityService 
            or BusinessRole.SwissDataSteward => _allLevels,

            BusinessRole.LocalDataSteward => _userContextService.UserBelongsToAgency(agencyIdentifier) 
                ? _allLevels 
                : [],

            BusinessRole.Submitter 
            or BusinessRole.StewardshipOrganisationViewer
            or BusinessRole.Unknown => [],
            _ => throw new NotSupportedException($"The business role '{userBusinessRole}' is not supported.")
        };

        return allowedValidations.Except([currentLevel]);
    }
}
