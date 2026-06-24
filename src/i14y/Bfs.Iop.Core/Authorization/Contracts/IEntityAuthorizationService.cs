using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Authorization.Contracts;

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
