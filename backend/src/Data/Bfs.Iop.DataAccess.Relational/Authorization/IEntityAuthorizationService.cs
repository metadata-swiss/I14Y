using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.DataAccess.Relational.Authorization;

internal interface IEntityAuthorizationService
{
    void EnsureUserHasRoleToCreateEntities(IEnumerable<BusinessRole> allowedBusinessRolesToCreateEntity);

    void EnsureUserCanCreateEntity(
        string entityPublisherIdentifier,
        IEnumerable<BusinessRole> allowedBusinessRolesToCreateEntity);

    void EnsureUserHasRoleToUpdateEntities(IEnumerable<BusinessRole> allowedBusinessRolesToUpdateEntity);

    void EnsureUserCanUpdateEntity(
        EntityBase entity,
        IEnumerable<BusinessRole> allowedBusinessRolesToUpdateEntity);

    void EnsureUserHasRoleToDeleteEntities(IEnumerable<BusinessRole> allowedBusinessRolesToDeleteEntity);

    void EnsureUserCanDeleteEntity(
        EntityBase entity,
        IEnumerable<BusinessRole> allowedBusinessRolesToDeleteEntity);
}
