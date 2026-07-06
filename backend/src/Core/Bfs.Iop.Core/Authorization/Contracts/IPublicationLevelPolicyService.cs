using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Authorization.Contracts;

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
