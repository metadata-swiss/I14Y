using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Tools;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Bfs.Iop.Core.Authorization.Contracts;

namespace Bfs.Iop.Core.Services;

internal abstract class PublishableEntityServiceBase<T> : AuthorizedEntityServiceBase<T>, IPublishableEntityService where T : PublishableEntityBase
{
    private readonly IPublicationLevelPolicyService _publicationLevelPolicyService;
    private readonly IRegistrationStatusPolicyService _registrationStatusPolicyService;
    
    protected readonly IPublishableEntityAuthorizationService _publishableEntityAuthorizationService;
    protected readonly IopDbContext _dbContext;
    protected readonly ICatalogIndexService _catalogIndexService;
    protected readonly IIdentifierGenerator _identifierGenerator;

    protected PublishableEntityServiceBase(
        IopDbContext dbContext,
        ICatalogIndexService catalogIndexService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        IIdentifierGenerator identifierGenerator,
        IUserContextService userContextService) : base(authorizationService, userContextService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));

        _publicationLevelPolicyService = publicationLevelPolicyService ??
            throw new ArgumentNullException(nameof(publicationLevelPolicyService));

        _registrationStatusPolicyService = registrationStatusPolicyService ??
            throw new ArgumentNullException(nameof(registrationStatusPolicyService));

        _publishableEntityAuthorizationService = authorizationService ??
            throw new ArgumentNullException(nameof(authorizationService));

        _identifierGenerator = identifierGenerator ?? throw new ArgumentNullException(nameof(identifierGenerator));
    }

    public async Task<PublicationLevelInfoModel> GetPublicationLevelAndProposalAndUserAllowedValues(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        var currentLevel = entity.PublicationLevel;
        var currentProposal = entity.PublicationLevelProposal;
        var agentIdentifier = entity.Publisher.Identifier;

        var userAllowedProposals = _publicationLevelPolicyService.GetUserAllowedProposals(
            currentLevel,
            currentProposal,
            agentIdentifier,
            out var canUserRevertProposal);

        var userAllowedLevels = _publicationLevelPolicyService.GetUserAllowedValidations(
            currentLevel,
            agentIdentifier);

        return new()
        {
            AllowedLevels = userAllowedLevels,
            AllowedProposals = userAllowedProposals,
            CanUserRevertProposal = canUserRevertProposal,
            Level = currentLevel,
            Proposal = currentProposal,
        };
    }

    public async Task<RegistrationStatusInfoModel> GetRegistrationStatusAndProposalAndUserAllowedValues(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        var currentStatus = entity.RegistrationStatus;
        var currentProposal = entity.RegistrationStatusProposal;
        var currentPublicationLevel = entity.PublicationLevel;
        var agentIdentifier = entity.Publisher.Identifier;

        var userAllowedProposals = _registrationStatusPolicyService.GetUserAllowedProposals(
            currentStatus,
            currentProposal,
            agentIdentifier,
            out var canUserRevertProposal);

        var userAllowedLevels = _registrationStatusPolicyService.GetUserAllowedValidations(
            currentStatus,
            currentPublicationLevel,
            agentIdentifier);

        return new()
        {
            AllowedStatuses = userAllowedLevels,
            AllowedProposals = userAllowedProposals,
            CanUserRevertProposal = canUserRevertProposal,
            Status = currentStatus,
            Proposal = currentProposal,
        };
    }

    public async Task<IEnumerable<AgentStatisticsResult>> GetPublishersStatistics(
        IEnumerable<AgentModel> publishers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publishers, nameof(publishers));

        var query = CreateGetAuthorizedEntitiesQuery(filter: null, asNoTracking: true, EntityIncludeLevel.Minimal);

        var catalogType = query switch
        {
            _ when query is IQueryable<Dataset> _ => SearchResourceType.Dataset,
            _ when query is IQueryable<DataService> _ => SearchResourceType.DataService,
            _ when query is IQueryable<IopConcept> _ => SearchResourceType.Concept,
            _ when query is IQueryable<PublicService> _ => SearchResourceType.PublicService,
            _ when query is IQueryable<MappingTable> _ => SearchResourceType.MappingTable,
            _ => throw new NotSupportedException($"The type '{typeof(T)}' is not supported.")
        };

        var dic = (await query
                .GroupBy(x => x.PublisherId)
                .ToDictionaryAsync(x => x.Key, x => x.Count(), cancellationToken)).AsReadOnly();

        return publishers.Select(agent => new AgentStatisticsResult()
        {
            Publisher = agent,
            Types =
            [
                new()
                {
                    Value = catalogType,
                    Count = dic.TryGetValue(agent.Id, out int value) ? value : 0
                }
            ]
        });
    }

    public async Task UpdateRegistrationStatusProposal(
        Guid id,
        RegistrationStatus? proposal,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        _publishableEntityAuthorizationService.EnsureRegistrationStatusProposalCanBeUpdated(entity, proposal);

        entity.RegistrationStatusProposal = proposal;

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(id, cancellationToken);
    }

    public async Task UpdateRegistrationStatus(
        Guid id,
        RegistrationStatus status,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        _publishableEntityAuthorizationService.EnsureRegistrationStatusCanBeUpdated(entity, status);

        entity.RegistrationStatus = status;
        entity.RegistrationStatusProposal = null;

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(id, cancellationToken);
    }

    public async Task UpdatePublicationLevelProposal(
        Guid id,
        PublicationLevel? proposal,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        _publishableEntityAuthorizationService.EnsurePublicationLevelProposalCanBeUpdated(entity, proposal);

        entity.PublicationLevelProposal = proposal;

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(id, cancellationToken);
    }

    public async Task UpdatePublicationLevel(
        Guid id,
        PublicationLevel level,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        _publishableEntityAuthorizationService.EnsurePublicationLevelCanBeUpdated(entity, level);

        entity.PublicationLevel = level;
        entity.PublicationLevelProposal = null;

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(id, cancellationToken);
    }

    protected async Task<AllowActionResult> GetUserAllowVersionInfo(Guid id, CancellationToken cancellationToken = default)
    {
        AllowActionResult allowVersion;

        try
        {
            var entity = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

            EnsureUserCanCreateEntity(entity.Publisher.Identifier);

            allowVersion = new()
            {
                Value = true,
                ActionType = AllowActionType.Version,
            };
        }
        catch (Exception exception)
        when (exception is IAllowActionInfoException ex)
        {
            allowVersion = new()
            {
                ActionType = AllowActionType.Version,
                Value = false,
                Message = ex.Message,
                MessageDetailsCode = (int)ex.AllowActionMessageCode
            };
        }

        return allowVersion;
    }

    protected void AppendUserReadAuthorizationConditionToDatabaseQuery(ref IQueryable<T> query) => 
        query = _publishableEntityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(query);

    protected static void EnsureFirstIdentifierIsUnchanged(
        string? existingFirst,
        string? incomingFirst,
        PublicationLevel publicationLevel)
    {
        if (publicationLevel == PublicationLevel.Public
            && existingFirst is not null
            && incomingFirst != existingFirst)
        {
            throw new ValidationException(
            [
                new ValidationFailure("Identifiers",
                    "The first identifier cannot be changed once the publication status is set to public.")
            ]);
        }
    }

    protected abstract IQueryable<T> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<T, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel);

    protected abstract Task UpdateIndex(Guid id, CancellationToken cancellationToken);

    protected override async Task<T> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = CreateGetAuthorizedEntitiesQuery(filter: null, asNoTracking, entityIncludeLevel); 

        var entity = await query.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (entity is null)
        {
            throw (await _dbContext.FindAsync<T>(id, cancellationToken)) is null 
                ? new NotFoundException("No resource has been found.")
                : userHasValidToken
                    ? new ForbiddenException("The resource is forbidden.")
                    : new UnauthorizedException("No authorization to access the resource.");
        }

        return entity;
    }
}
