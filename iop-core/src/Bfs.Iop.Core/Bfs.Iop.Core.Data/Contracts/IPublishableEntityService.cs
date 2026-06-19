using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IPublishableEntityService : IAuthorizedEntityService
{
    Task<PublicationLevelInfoModel> GetPublicationLevelAndProposalAndUserAllowedValues(
        Guid id,
        CancellationToken cancellationToken);

    Task<RegistrationStatusInfoModel> GetRegistrationStatusAndProposalAndUserAllowedValues(
        Guid id,
        CancellationToken cancellationToken);

    Task<IEnumerable<AgentStatisticsResult>> GetPublishersStatistics(
        IEnumerable<AgentModel> publishers,
        CancellationToken cancellationToken = default);

    Task UpdateRegistrationStatusProposal(
        Guid id,
        RegistrationStatus? proposal,
        CancellationToken cancellationToken);

    Task UpdateRegistrationStatus(
        Guid id,
        RegistrationStatus status,
        CancellationToken cancellationToken);

    Task UpdatePublicationLevelProposal(
        Guid id,
        PublicationLevel? proposal,
        CancellationToken cancellationToken);

    Task UpdatePublicationLevel(
        Guid id,
        PublicationLevel level,
        CancellationToken cancellationToken);
}
