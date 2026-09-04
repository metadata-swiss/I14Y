using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Authorization;

internal interface IPublishableEntityAuthorizationService : IEntityAuthorizationService
{
    IQueryable<T> AppendUserReadAuthorizationConditionToDatabaseQuery<T>(IQueryable<T> query) where T : PublishableEntityBase;

    void EnsureUserCanReadPublishableEntity(PublishableEntityBase entity);

    void EnsureRegistrationStatusProposalCanBeUpdated(PublishableEntityBase entity, RegistrationStatus? proposal);

    void EnsureRegistrationStatusCanBeUpdated(PublishableEntityBase entity, RegistrationStatus status);

    void EnsurePublicationLevelProposalCanBeUpdated(PublishableEntityBase entity, PublicationLevel? proposal);

    void EnsurePublicationLevelCanBeUpdated(PublishableEntityBase entity, PublicationLevel level);
}
