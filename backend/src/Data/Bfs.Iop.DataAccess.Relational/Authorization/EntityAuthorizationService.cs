using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.DataAccess.Relational.Authorization;

internal class EntityAuthorizationService : IEntityAuthorizationService
{
    protected readonly IUserContextService _userContextService;

    public EntityAuthorizationService(IUserContextService userContextService) =>
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));

    public void EnsureUserHasRoleToCreateEntities(IEnumerable<BusinessRole> allowedBusinessRolesToCreateEntity)
    {
        ArgumentNullException.ThrowIfNull(allowedBusinessRolesToCreateEntity, nameof(allowedBusinessRolesToCreateEntity));

        EnsureUserHasRole(allowedBusinessRolesToCreateEntity);
    }

    public virtual void EnsureUserCanCreateEntity(
        string entityPublisherIdentifier,
        IEnumerable<BusinessRole> allowedBusinessRolesToCreateEntity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityPublisherIdentifier, nameof(entityPublisherIdentifier));

        EnsureUserHasRoleToCreateEntities(allowedBusinessRolesToCreateEntity);

        EnsureUserBelongsToPublisher(entityPublisherIdentifier);
    }

    public void EnsureUserHasRoleToUpdateEntities(IEnumerable<BusinessRole> allowedBusinessRolesToUpdateEntity)
    {
        ArgumentNullException.ThrowIfNull(allowedBusinessRolesToUpdateEntity, nameof(allowedBusinessRolesToUpdateEntity));

        EnsureUserHasRole(allowedBusinessRolesToUpdateEntity);
    }

    public virtual void EnsureUserCanUpdateEntity(
        EntityBase entity,
        IEnumerable<BusinessRole> allowedBusinessRolesToUpdateEntity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        EnsureUserHasRoleToUpdateEntities(allowedBusinessRolesToUpdateEntity);

        if (entity is IOwnedEntity ownedEntity)
        {
            EnsureUserBelongsToPublisher(ownedEntity.Publisher.Identifier);
        }
    }

    public void EnsureUserHasRoleToDeleteEntities(IEnumerable<BusinessRole> allowedBusinessRolesToDeleteEntity)
    {
        ArgumentNullException.ThrowIfNull(allowedBusinessRolesToDeleteEntity, nameof(allowedBusinessRolesToDeleteEntity));

        EnsureUserHasRole(allowedBusinessRolesToDeleteEntity);
    }

    public virtual void EnsureUserCanDeleteEntity(
        EntityBase entity,
        IEnumerable<BusinessRole> allowedBusinessRolesToDeleteEntity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        EnsureUserHasRoleToDeleteEntities(allowedBusinessRolesToDeleteEntity);

        if (entity is IOwnedEntity ownedEntity)
        {
            EnsureUserBelongsToPublisher(ownedEntity.Publisher.Identifier);
        }
    }

    protected void EnsureUserBelongsToPublisher(string publisherIdentifier)
    {
        const string unauthorizedExceptionMessage = "Must have a valid token.";
        var forbiddenExceptionMessage = $"User does not belong to organisation '{publisherIdentifier}'";

        var userBelongsToPublisher = _userContextService.UserBelongsToAgency(publisherIdentifier);

        if (!userBelongsToPublisher)
        {
            var userHasValidToken = _userContextService.IsUserTokenValid();

            throw userHasValidToken
                ? new ForbiddenException(forbiddenExceptionMessage)
                : new UnauthorizedException(unauthorizedExceptionMessage);
        }
    }

    protected void EnsureUserHasRole(IEnumerable<BusinessRole> allowedRoles)
    {
        const string forbiddenExceptionMessage = "Business role does not allow the operation.";
        const string unauthorizedExceptionMessage = "Must have a valid token.";

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        var userHasBusinessRole = allowedRoles.Contains(userBusinessRole);

        if (!userHasBusinessRole)
        {
            var userHasValidToken = _userContextService.IsUserTokenValid();

            throw userHasValidToken
                ? new ForbiddenException(forbiddenExceptionMessage)
                : new UnauthorizedException(unauthorizedExceptionMessage);
        }
    }
}
