using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Authorization;

internal interface IRegistrationStatusPolicyService
{
    IEnumerable<RegistrationStatus> GetUserAllowedProposals(
        RegistrationStatus currentStatus,
        RegistrationStatus? currentProposal,
        string agencyIdentifier,
        out bool canUserRevertProposal);

    IEnumerable<RegistrationStatus> GetUserAllowedValidations(
        RegistrationStatus currentStatus,
        PublicationLevel currentPublicationLevel,
        string agencyIdentifier);
}
