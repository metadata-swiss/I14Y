using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Infrastructure.Security.Services;

namespace Bfs.Iop.Core.Authorization;

internal sealed class PublishableEntityAuthorizationService : EntityAuthorizationService, IPublishableEntityAuthorizationService
{
    private readonly IPublicationLevelPolicyService _publicationLevelPolicyService;
    private readonly IRegistrationStatusPolicyService _registrationStatusPolicyService;

    public PublishableEntityAuthorizationService(
        IUserContextService userContextService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService) : base(userContextService)
    {
        _publicationLevelPolicyService = publicationLevelPolicyService ??
            throw new ArgumentNullException(nameof(publicationLevelPolicyService));

        _registrationStatusPolicyService = registrationStatusPolicyService ??
            throw new ArgumentNullException(nameof(registrationStatusPolicyService));
    }

    public IQueryable<T> AppendUserReadAuthorizationConditionToDatabaseQuery<T>(IQueryable<T> query) where T : PublishableEntityBase
    {
        var userBusinessRole = _userContextService.GetUserBusinessRole();
        var userAgencies = _userContextService.GetUserAgencies();

        return userBusinessRole switch
        {
            BusinessRole.SwissDataSteward or
            BusinessRole.InteroperabilityService => query,

            BusinessRole.Unknown => query.Where(x => x.PublicationLevel == PublicationLevel.Public),

            BusinessRole.StewardshipOrganisationViewer or
            BusinessRole.LocalDataSteward or
            BusinessRole.Submitter => query.Where(x => 
                x.PublicationLevel == PublicationLevel.Public ||
                (x.PublicationLevel == PublicationLevel.Internal && userAgencies.Contains(x.Publisher.Identifier))),

            _ => throw new NotSupportedException($"The business role '{userBusinessRole}' is not supported."),
        };
    }

    public void EnsureUserCanReadPublishableEntity(PublishableEntityBase entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        if (entity.PublicationLevel is PublicationLevel.Public ||
            userBusinessRole is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return;
        }

        if (userBusinessRole is BusinessRole.Unknown)
        {
            throw new UnauthorizedException($"No authorization to read the resource.");
        }

        EnsureUserBelongsToPublisher(entity.Publisher.Identifier);
    }

    public override void EnsureUserCanUpdateEntity(EntityBase entity, IEnumerable<BusinessRole> allowedBusinessRolesToUpdateEntity)
    {
        base.EnsureUserCanUpdateEntity(entity, allowedBusinessRolesToUpdateEntity);

        EnsureAuthorizedUserCanUpdateEntity((PublishableEntityBase)entity);
    }

    public override void EnsureUserCanDeleteEntity(EntityBase entity, IEnumerable<BusinessRole> allowedBusinessRolesToDeleteEntity)
    {
        base.EnsureUserCanDeleteEntity(entity, allowedBusinessRolesToDeleteEntity);

        var publishableEntity = (PublishableEntityBase)entity;

        EnsureAuthorizedUserCanUpdateEntity(publishableEntity);

        if (publishableEntity.PublicationLevel is not PublicationLevel.Internal)
        {
            throw new MethodNotAllowedException(
                $"The resource cannot be deleted. " +
                $"The publication level must be '{PublicationLevel.Internal}'.", AllowActionMessageCode.ResourceIsPublic);
        }
    }

    public void EnsureRegistrationStatusProposalCanBeUpdated(PublishableEntityBase entity, RegistrationStatus? proposal)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        proposal?.EnsureValueIsValid();

        if (entity.RegistrationStatusProposal == proposal)
        {
            throw new MethodNotAllowedException(proposal is null
                ? "The resource has no registration status proposal to revert."
                : $"The resource already has its registration status proposal set to '{proposal}'.");
        }

        if (entity.RegistrationStatus == proposal)
        {
            throw new MethodNotAllowedException(
                $"The resource already has its registration status set to '{proposal}'.");
        }

        var publisherIdentifier = entity.Publisher.Identifier;

        var allowedProposals = _registrationStatusPolicyService.GetUserAllowedProposals(
            entity.RegistrationStatus,
            entity.RegistrationStatusProposal,
            publisherIdentifier,
            out var canUserRevertProposal);

        if (proposal is null && !canUserRevertProposal)
        {
            throw new ForbiddenException(
                "No authorization to revert the registration status proposal of the resource.");
        }

        var isValidProposal = proposal is null || allowedProposals.Contains(proposal.Value);

        if (!isValidProposal)
        {
            throw new ForbiddenException(
                $"No authorization to set registration status proposal of the resource to '{proposal}'.");
        }
    }

    public void EnsureRegistrationStatusCanBeUpdated(PublishableEntityBase entity, RegistrationStatus status)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        status.EnsureValueIsValid();

        if (entity.RegistrationStatus == status)
        {
            throw new MethodNotAllowedException(
                $"The resource already has its registration status set to '{status}'.");
        }

        var publisherIdentifier = entity.Publisher.Identifier;

        var isValidStatus = _registrationStatusPolicyService
            .GetUserAllowedValidations(entity.RegistrationStatus, entity.PublicationLevel, publisherIdentifier)
            .Contains(status);

        if (!isValidStatus)
        {
            throw new ForbiddenException(
                $"No authorization to set registration status of the resource to '{status}'.");
        }
    }

    public void EnsurePublicationLevelProposalCanBeUpdated(PublishableEntityBase entity, PublicationLevel? proposal)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        proposal?.EnsureValueIsValid();

        if (entity.PublicationLevelProposal == proposal)
        {
            throw new MethodNotAllowedException(proposal is null
                ? "The resource has no publication level proposal to revert."
                : $"The resource already has its publication level proposal set to '{proposal}'.");
        }

        if (entity.PublicationLevel == proposal)
        {
            throw new MethodNotAllowedException(
                $"The resource already has its publication level set to '{proposal}'.");
        }

        var publisherIdentifier = entity.Publisher.Identifier;

        var allowedProposals = _publicationLevelPolicyService.GetUserAllowedProposals(
            entity.PublicationLevel,
            entity.PublicationLevelProposal,
            publisherIdentifier,
            out var canUserRevertProposal);

        var isValidProposal = proposal is null || allowedProposals.Contains(proposal.Value);

        if (proposal is null && !canUserRevertProposal)
        {
            throw new ForbiddenException(
                "No authorization to revert the publication level proposal of the resource.");
        }

        if (!isValidProposal)
        {
            throw new ForbiddenException(
                $"No authorization to set publication level proposal of the resource to '{proposal}'.");
        }
    }

    public void EnsurePublicationLevelCanBeUpdated(PublishableEntityBase entity, PublicationLevel level)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        level.EnsureValueIsValid();

        if (entity.PublicationLevel == level)
        {
            throw new MethodNotAllowedException(
                $"The resource already has its publication level set to '{level}'.");
        }

        var publisherIdentifier = entity.Publisher.Identifier;

        var isValidLevel = _publicationLevelPolicyService
            .GetUserAllowedValidations(entity.PublicationLevel, publisherIdentifier)
            .Contains(level);

        if (!isValidLevel)
        {
            throw new ForbiddenException(
                $"No authorization to set publication level of the resource to '{level}'.");
        }
    }

    private void EnsureAuthorizedUserCanUpdateEntity(PublishableEntityBase entity)
    {
        var entityPublisherIdentifier = entity.Publisher.Identifier;

        EnsureUserBelongsToPublisher(entityPublisherIdentifier);

        var userBusinessRole = _userContextService.GetUserBusinessRole();

        if (userBusinessRole is BusinessRole.Submitter)
        {
            EnsureAuthorizedSubmitterCanUpdateDeletePublishableEntity(entity);
        }
    }

    private static void EnsureAuthorizedSubmitterCanUpdateDeletePublishableEntity(PublishableEntityBase entity)
    {
        var isEntityEditable = entity.RegistrationStatus is RegistrationStatus.Incomplete or RegistrationStatus.Candidate;

        if (!isEntityEditable)
        {
            throw new ForbiddenException($"No authorization to update or delete the resource " +
                $"due to its registration status '{entity.RegistrationStatus}'.");
        }
    }
}
