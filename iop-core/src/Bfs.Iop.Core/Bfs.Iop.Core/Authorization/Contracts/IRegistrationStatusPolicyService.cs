using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Authorization.Contracts;

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
