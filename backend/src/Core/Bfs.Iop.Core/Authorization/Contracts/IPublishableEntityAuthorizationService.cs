using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Authorization.Contracts;

internal interface IPublishableEntityAuthorizationService : IEntityAuthorizationService
{
    IQueryable<T> AppendUserReadAuthorizationConditionToDatabaseQuery<T>(IQueryable<T> query) where T : PublishableEntityBase;

    void EnsureUserCanReadPublishableEntity(PublishableEntityBase entity);

    void EnsureRegistrationStatusProposalCanBeUpdated(PublishableEntityBase entity, RegistrationStatus? proposal);

    void EnsureRegistrationStatusCanBeUpdated(PublishableEntityBase entity, RegistrationStatus status);

    void EnsurePublicationLevelProposalCanBeUpdated(PublishableEntityBase entity, PublicationLevel? proposal);

    void EnsurePublicationLevelCanBeUpdated(PublishableEntityBase entity, PublicationLevel level);
}
