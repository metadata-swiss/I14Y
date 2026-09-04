using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal abstract class AuthorizedEntityServiceBase<T> : IAuthorizedEntityService where T : EntityBase
{
    protected readonly IEntityAuthorizationService _entityAuthorizationService;
    protected readonly IUserContextService _userContextService;

    protected virtual IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDeleteEntity =>
        [
        BusinessRole.Submitter,
        BusinessRole.LocalDataSteward,
        BusinessRole.InteroperabilityService,
        BusinessRole.SwissDataSteward
        ];

    protected AuthorizedEntityServiceBase(
        IEntityAuthorizationService entityAuthorizationService,
        IUserContextService userContextService)
    {
        _entityAuthorizationService = entityAuthorizationService ?? 
            throw new ArgumentNullException(nameof(entityAuthorizationService));

        _userContextService = userContextService ??
            throw new ArgumentNullException(nameof(userContextService));
    }

    public virtual async Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        T? entity;
        AllowActionResult readAllowResult;
        AllowActionResult updateAllowResult;
        AllowActionResult deleteAllowResult;

        try
        {
            entity = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: default);
            readAllowResult = new() 
            {
                ActionType = AllowActionType.Read,
                Value = true
            };
        }
        catch (Exception exception) 
        when (exception is IAllowActionInfoException ex)
        {
            readAllowResult = new()
            {
                ActionType = AllowActionType.Read,
                Value = false,
                Message = exception.Message,
                MessageDetailsCode = (int)ex.AllowActionMessageCode
            };

            entity = null;
        }

        if (entity is not null)
        {
            try
            {
                EnsureUserCanUpdateEntity(entity);
                updateAllowResult = new() 
                {
                    ActionType = AllowActionType.Edit,
                    Value = true 
                };
            }
            catch (Exception exception)
            when (exception is IAllowActionInfoException ex)
            {
                updateAllowResult = new()
                {
                    ActionType = AllowActionType.Edit,
                    Value = false,
                    Message = exception.Message,
                    MessageDetailsCode = (int)ex.AllowActionMessageCode
                };
            }

            try
            {
                EnsureUserCanDeleteEntity(entity);
                deleteAllowResult = new()
                {
                    ActionType = AllowActionType.Delete,
                    Value = true 
                };
            }
            catch (Exception exception) 
            when (exception is IAllowActionInfoException ex)
            {
                deleteAllowResult = new()
                {
                    ActionType = AllowActionType.Delete,
                    Value = false,
                    Message = ex.Message,
                    MessageDetailsCode = (int)ex.AllowActionMessageCode
                };
            }
        }
        else
        {
            updateAllowResult = readAllowResult with { ActionType = AllowActionType.Edit };
            deleteAllowResult = readAllowResult with { ActionType = AllowActionType.Delete };
        }

        return [
            readAllowResult,
            updateAllowResult, 
            deleteAllowResult
            ]
        ;
    }

    public virtual Task<AllowActionResult> GetUserAllowCreateInfo(
        CancellationToken cancellationToken = default)
    {
        AllowActionResult allowCreateResult;

        try
        {
            EnsureUserCanCreateEntities();

            allowCreateResult = new()
            {
                ActionType = AllowActionType.Create,
                Value = true 
            };
        }
        catch (Exception exception)
        when (exception is IAllowActionInfoException ex)
        {
            allowCreateResult = new()
            {
                ActionType = AllowActionType.Create,
                Message = ex.Message,
                Value = false,
                MessageDetailsCode = (int)ex.AllowActionMessageCode
            };
        }

        return Task.FromResult(allowCreateResult);
    }

    protected virtual void EnsureUserCanCreateEntities() =>
        _entityAuthorizationService.EnsureUserHasRoleToCreateEntities(AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected virtual void EnsureUserCanCreateEntity(string entityPublisherIdentifier) =>
        _entityAuthorizationService.EnsureUserCanCreateEntity(entityPublisherIdentifier, AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected virtual void EnsureUserCanUpdateEntities() =>
        _entityAuthorizationService.EnsureUserHasRoleToUpdateEntities(AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected virtual void EnsureUserCanUpdateEntity(T entity) =>
        _entityAuthorizationService.EnsureUserCanUpdateEntity(entity, AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected virtual void EnsureUserCanDeleteEntities() =>
        _entityAuthorizationService.EnsureUserHasRoleToDeleteEntities(AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected virtual void EnsureUserCanDeleteEntity(T entity) =>
        _entityAuthorizationService.EnsureUserCanDeleteEntity(entity, AllowedBusinessRolesToCreateUpdateDeleteEntity);

    protected abstract Task<T> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default);
}
