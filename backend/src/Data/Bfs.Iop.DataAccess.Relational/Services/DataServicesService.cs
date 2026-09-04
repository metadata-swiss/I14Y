using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Authorization;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class DataServicesService : PublishableEntityServiceBase<DataService>, IDataServicesService
{
    private readonly IAgentsService _agentsService;
    private readonly IIopPersonsService _iopPersonService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IValidator<DataServiceInputModel> _dataServiceInputModelValidator;

    public DataServicesService(
        IAgentsService agentsService,
        IIopPersonsService iopPersonsService,
        IVocabulariesService vocabulariesService,
        IopDbContext dbContext,
        IUserContextService userContextService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        IIdentifierGenerator identifierGenerator,
        IValidator<DataServiceInputModel> dataServiceInputModelValidator) : base(
            dbContext,
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            authorizationService,
            identifierGenerator,
            userContextService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _iopPersonService = iopPersonsService ?? throw new ArgumentNullException(nameof(iopPersonsService));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
        _dataServiceInputModelValidator = dataServiceInputModelValidator 
            ?? throw new ArgumentNullException(nameof(dataServiceInputModelValidator));
    }

    public async Task<DataServiceModel> GetDataService(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.All, cancellationToken);

        return entity.MapToDataServiceModel(_vocabulariesService);
    }

    public async Task<DataServiceModel> GetDataServiceByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        var id = (await _dbContext.DataServices.SingleOrDefaultAsync(x => x.Identifiers.Contains(identifier), cancellationToken))?.Id ??
            throw new NotFoundException("No resource has been found.");

        return await GetDataService(id, cancellationToken);
    }

    public async Task<PagedResult<DataServiceModel>> GetDataServicesServingDataset(
        Guid datasetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var relations = await _dbContext.DataServiceDatasetRelations
            .Where(x => x.DatasetId == datasetId)
            .ToListAsync(cancellationToken);

        var ids = relations.Select(x => x.DataServiceId);

        var query = CreateGetAuthorizedEntitiesQuery(x => ids.Contains(x.Id), asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.MapToDataServiceModel(_vocabulariesService))
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.AsReadOnly(),
            TotalCount = totalResults
        };
    }

    public async Task<PagedResult<DataServiceModel>> GetDataServicesFromDistributionAccessServices(
        Guid distributionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var relations = await _dbContext.DistributionDataServiceRelations
            .Where(x => x.DistributionId == distributionId)
            .ToListAsync(cancellationToken);

        var ids = relations.Select(x => x.DataServiceId);

        var query = CreateGetAuthorizedEntitiesQuery(x => ids.Contains(x.Id), asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.MapToDataServiceModel(_vocabulariesService))
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.AsReadOnly(),
            TotalCount = totalResults
        };
    }

    public async Task<PagedResult<DataServiceModel>> GetDataServices(
        string? accessRights,
        string? dataServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<DataService, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(accessRights) || x.AccessRights == accessRights) &&
            (string.IsNullOrWhiteSpace(dataServiceIdentifier) || x.Identifiers.Contains(dataServiceIdentifier)) &&
            (string.IsNullOrWhiteSpace(publisherIdentifier) || x.Publisher.Identifier == publisherIdentifier) &&
            (!publicationLevel.HasValue || x.PublicationLevel == publicationLevel.Value) &&
            (!registrationStatus.HasValue || x.RegistrationStatus == registrationStatus.Value);

        var query = CreateGetAuthorizedEntitiesQuery(filter, asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToDataServiceModel(_vocabulariesService)).ToList().AsReadOnly(),
            TotalCount = totalResults
        };
    }

    public async Task<PagedResult<DataServiceModel>> GetDataServiceNextVersions(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        // Ensure user can read this dataset.
        _ = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.Minimal, cancellationToken);

        var query = CreateGetAuthorizedEntitiesQuery(x => x.PreviousVersionId == id, asNoTracking: true, EntityIncludeLevel.All);

        var count = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? count : pageSize,
            Results = results.Select(x => x.MapToDataServiceModel(_vocabulariesService)).ToList().AsReadOnly(),
            TotalCount = count
        };
    }

    public override async Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var allowedActions = await base.GetUserAllowActionInfo(id, cancellationToken);

        var allowVersion = await GetUserAllowVersionInfo(id, cancellationToken);

        return allowedActions.Concat([allowVersion]);
    }

    public async Task<Guid> AddDataService(DataServiceInputModel inputModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        EnsureUserCanCreateEntity(inputModel.Publisher.Identifier);

        EnsureInputModelIsValid(inputModel);

        var publisherId = await _agentsService.GetAgentId(inputModel.Publisher.Identifier, cancellationToken);

        Guid? responsiblePersonId = inputModel.ResponsiblePerson is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(inputModel.ResponsiblePerson.Email, cancellationToken)
            : null;

        Guid? responsiblePersonDeputyId = inputModel.ResponsibleDeputy is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(inputModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var dataService = inputModel.MapToDataService(
            publisherId,
            responsiblePersonId,
            responsiblePersonDeputyId,
            _identifierGenerator);

        await _dbContext.DataServices.AddAsync(dataService, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return dataService.Id;
    }

    public async Task UpdateDataService(Guid id, DataServiceInputModel updateModel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));

        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureInputModelIsValid(updateModel, id);

        EnsureFirstIdentifierIsUnchanged(entity.Identifiers.FirstOrDefault(), updateModel.Identifiers.FirstOrDefault(), entity.PublicationLevel);

        var publisherId = await _agentsService.GetAgentId(updateModel.Publisher.Identifier, cancellationToken);

        Guid? responsiblePersonId = updateModel.ResponsiblePerson is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(updateModel.ResponsiblePerson.Email, cancellationToken)
            : null;

        Guid? responsiblePersonDeputyId = updateModel.ResponsibleDeputy is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(updateModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        updateModel.MapToDataService(
            publisherId, 
            responsiblePersonId,
            responsiblePersonDeputyId,
            _identifierGenerator,
            entity);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDataService(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.DataServices.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override void EnsureUserCanDeleteEntity(DataService entity)
    {
        base.EnsureUserCanDeleteEntity(entity);

        var isPreviousVersion = _dbContext.DataServices.Any(x => x.PreviousVersionId == entity.Id);

        if (isPreviousVersion)
        {
            throw new MethodNotAllowedException(
                $"The resource cannot be deleted. " +
                $"It is referenced as a previous version from other data service.", AllowActionMessageCode.ResourceIsPreviousVersion);
        }

        var hasRelationWithDistribution = _dbContext.DistributionDataServiceRelations.Any(x => x.DataServiceId == entity.Id);

        if (hasRelationWithDistribution)
        {
            throw new MethodNotAllowedException(
                $"The resource cannot be deleted. " +
                $"It is referenced from other resources.", AllowActionMessageCode.ResourceReferenced);
        }
    }

    protected override IQueryable<DataService> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<DataService, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = asNoTracking
             ? _dbContext.DataServices.AsNoTracking()
             : _dbContext.DataServices.AsQueryable();

        query = entityIncludeLevel switch
        {
             EntityIncludeLevel.Minimal => query.Include(d => d.Publisher),
             _ => buildEntityIncludeLevelAllQuery(query, userHasValidToken),
        };

        AppendUserReadAuthorizationConditionToDatabaseQuery(ref query);

        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<DataService> buildEntityIncludeLevelAllQuery(IQueryable<DataService> query, bool userHasValidToken)
        {
            query = query
                .Include(d => d.ConformsTo)
                .Include(d => d.ContactPoint)
                .Include(d => d.Datasets)
                .Include(d => d.Documentation)
                .Include(d => d.EndpointDescription)
                .Include(d => d.EndpointUrl)
                .Include(d => d.Keyword)
                .Include(d => d.LandingPage)
                .Include(d => d.Publisher);

            if (userHasValidToken)
            {
                query = query
                    .Include(d => d.ResponsiblePerson)
                    .Include(d => d.ResponsibleDeputy);
            }

            return query;
        }
    }

    private void EnsureInputModelIsValid(DataServiceInputModel model, Guid? updateId = null)
    {
        var context = new ValidationContext<DataServiceInputModel>(model);

        if (updateId.HasValue)
        {
            context.RootContextData.Add(ValidationContextDataKeys.IdKey, updateId);
        }

        var result = _dataServiceInputModelValidator.Validate(context);

        // Ensure user has access to a previous version
        if (model.PreviousVersion is not null)
        {
            try
            {
                _ = GetEnsuredEntity(model.PreviousVersion.Id).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            when (ex is UnauthorizedException or ForbiddenException)
            {
                result.Errors.Add(new(nameof(model.PreviousVersion), ex.Message));
            }
        }

        // Ensure user has access to the datasets
        for (int i = 0; i < model.ServesDatasets.Count(); i++)
        {
            var item = model.ServesDatasets.ElementAt(i);
            var dataset = _dbContext.Find<Dataset>(item.Id);
            var propertyName = $"{nameof(model.ServesDatasets)}[{i}]";

            if (dataset is null)
            {
                result.Errors.Add(new(propertyName, $"No resource has been found."));
                continue;
            }

            try
            {
                _publishableEntityAuthorizationService.EnsureUserCanReadPublishableEntity(dataset);
            }
            catch (Exception ex)
            when (ex is UnauthorizedException or ForbiddenException)
            {
                result.Errors.Add(new(propertyName, ex.Message));
            }
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
