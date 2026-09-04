using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Authorization;

internal interface IPublicationLevelPolicyService
{
    IEnumerable<PublicationLevel> GetUserAllowedProposals(
        PublicationLevel currentLevel,
        PublicationLevel? currentProposal,
        string agencyIdentifier,
        out bool canUserRevertProposal);

    IEnumerable<PublicationLevel> GetUserAllowedValidations(
        PublicationLevel currentLevel,
        string agencyIdentifier);
}
