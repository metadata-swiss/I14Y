using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Services.Extensions;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Validation.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

internal sealed class DatasetsService : PublishableEntityServiceBase<Dataset>, IDatasetsService
{
    private readonly IAgentsService _agentsService;
    private readonly IIopPersonsService _iopPersonService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IValidator<DcatDatasetInputModel> _datasetInputModelValidator;
    private readonly IValidator<DatasetQualityInformationDataModel> _datasetQualityInformationDataModelValidator;

    public DatasetsService(
        IopDbContext dbContext,
        IAgentsService agentsService,
        ICatalogIndexService catalogIndexService,
        IIopPersonsService iopPersonsService,
        IValidator<DcatDatasetInputModel> datasetInputModelValidator,
        IValidator<DatasetQualityInformationDataModel> datasetQualityInformationDataModelValidator,
        IUserContextService userContextService,
        IVocabulariesService vocabulariesService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        IIdentifierGenerator identifierGenerator) : base(
            dbContext,
            catalogIndexService,
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            authorizationService,
            identifierGenerator,
            userContextService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _iopPersonService = iopPersonsService ?? throw new ArgumentNullException(nameof(iopPersonsService));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
        _datasetInputModelValidator = datasetInputModelValidator;
        _datasetQualityInformationDataModelValidator = datasetQualityInformationDataModelValidator;
    }

    public async Task<DcatDatasetModel> GetDataset(Guid id, CancellationToken cancellationToken)
    {
        var dataset = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.All, cancellationToken);

        return dataset.MapToDcatDatasetModel(_vocabulariesService);
    }

    public async Task<DcatDatasetModel> GetDatasetByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        var id = (await _dbContext.Datasets.SingleOrDefaultAsync(x => x.Identifier.Contains(identifier), cancellationToken))?.Id ??
            throw new NotFoundException("No resource has been found.");

        return await GetDataset(id, cancellationToken);
    }

    public async Task<PagedResult<DcatDatasetModel>> GetDatasetNextVersions(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        // Ensure user can read this dataset.
        var _ = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.Minimal, cancellationToken);

        var query = CreateGetAuthorizedEntitiesQuery(x => x.PreviousVersionId == id, asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Ensure security: only users with token can access this property
        if (!_userContextService.IsUserTokenValid())
        {
            MaskInternalProperties(results);
        }

        return new PagedResult<DcatDatasetModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToDcatDatasetModel(_vocabulariesService)),
            TotalCount = totalResults,
        };
    }

    public async Task<PagedResult<DcatDatasetModel>> GetDatasetsByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));

        var query = CreateGetAuthorizedEntitiesQuery(x => ids.Contains(x.Id), asNoTracking: true, EntityIncludeLevel.All);

        var results = await query
            .ToListAsync(cancellationToken);

        // Ensure security: only users with token can access this property
        if (!_userContextService.IsUserTokenValid())
        {
            MaskInternalProperties(results);
        }

        return new PagedResult<DcatDatasetModel>()
        {
            Page = 1,
            PageSize = results.Count,
            Results = results.Select(x => x.MapToDcatDatasetModel(_vocabulariesService)),
            TotalCount = results.Count,
        };
    }

    public async Task<PagedResult<DcatDatasetModel>> GetDatasets(
        string? accessRights,
        string? datasetIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<Dataset, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(accessRights) || x.AccessRights == accessRights) &&
            (string.IsNullOrWhiteSpace(datasetIdentifier) || x.Identifier.Contains(datasetIdentifier)) &&
            (string.IsNullOrWhiteSpace(publisherIdentifier) || x.Publisher.Identifier == publisherIdentifier) &&
            (!publicationLevel.HasValue || x.PublicationLevel == publicationLevel.Value) &&
            (!registrationStatus.HasValue || x.RegistrationStatus == registrationStatus.Value);

        var query = CreateGetAuthorizedEntitiesQuery(filter, asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Ensure security: only users with token can access this property
        if (!_userContextService.IsUserTokenValid())
        {
            MaskInternalProperties(results);
        }

        return new PagedResult<DcatDatasetModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToDcatDatasetModel(_vocabulariesService)),
            TotalCount = totalResults,
        };
    }

    public async IAsyncEnumerable<IEnumerable<DcatDatasetModel>> GetDatasetsForIndexInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Datasets.AsNoTracking();

        query = query
            .Include(d => d.Distributions)
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson)
            .Include(d => d.ContactPoint);

        var batch = new List<DcatDatasetModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in query.ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(item.MapToDcatDatasetModel(_vocabulariesService));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<DcatDatasetModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<PagedResult<DatasetQualityQuestionModel>> GetQualityQuestions(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var totalCount = await _dbContext.DatasetQualityQuestions.CountAsync(cancellationToken);

        var results = await _dbContext.DatasetQualityQuestions
            .Include(d => d.AnswerOptions)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DatasetQualityQuestionModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = results.Select(x => x.MapToDatasetQualityQuestionModel()),
            TotalCount = totalCount,
        };
    }

    public async Task<DatasetQualityInformationDataModel> GetDatasetQualityInformation(Guid id, CancellationToken cancellationToken = default)
    {
        // Ensure the user can read the dataset
        _ = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        var links = await _dbContext.DatasetQualityInformationLinks
            .Where(x => x.DatasetId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var qualityInformations = await _dbContext.DatasetQualityInformation
            .Where(x => x.DatasetId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new DatasetQualityInformationDataModel()
        {
            DatasetId = id,
            Documentation = links.Select(x => x.MapToDatasetQualityInformationLinkModel()),
            QualityInformations = qualityInformations.Select(x => x.MapToDatasetQualityInformationModel())
        };
    }

    public async Task<Guid> AddDataset(DcatDatasetInputModel inputModel, CancellationToken cancellationToken)
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

        var attributionsAgentsMappingTable = _agentsService.GetAgentsIdentifiersIdsDictionary(
            inputModel.QualifiedAttributions.Select(x => x.Agent.Identifier));

        var dataset = inputModel.MapToDataset(
            publisherId,
            responsiblePersonId,
            responsiblePersonDeputyId,
            attributionsAgentsMappingTable,
            _identifierGenerator);

        await _dbContext.Datasets.AddAsync(dataset, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(dataset.Id, cancellationToken);

        return dataset.Id;
    }

    public async Task AddDatasetQualityInformation(
        Guid id,
        DatasetQualityInformationDataModel inputModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        var dataset = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dataset);

        EnsureDatasetQualityInformationDataModelIsValid(inputModel);

        // Check if the quality information already exists for this dataset
        if (_dbContext.DatasetQualityInformation.Any(x => x.DatasetId == id))
        {
            throw new MethodNotAllowedException(
                $"A quality information for the dataset with the id '{id}' already exists. " +
                $"Please update the existing quality information instead.");
        }

        var links = inputModel.Documentation.Select(x => x.MapToDatasetQualityInformationLink(id));
        var info = inputModel.QualityInformations.Select(x => x.MapToDatasetQualityInformation(id));

        await _dbContext.DatasetQualityInformationLinks.AddRangeAsync(links, cancellationToken);
        await _dbContext.DatasetQualityInformation.AddRangeAsync(info, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateDataset(Guid id, DcatDatasetInputModel updateModel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));

        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureInputModelIsValid(updateModel, id);

        EnsureFirstIdentifierIsUnchanged(entity.Identifier.FirstOrDefault(), updateModel.Identifiers.FirstOrDefault(), entity.PublicationLevel);

        var publisherId = await _agentsService.GetAgentId(updateModel.Publisher.Identifier, cancellationToken);

        Guid? responsiblePersonId = updateModel.ResponsiblePerson is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(updateModel.ResponsiblePerson.Email, cancellationToken)
            : null;

        Guid? responsiblePersonDeputyId = updateModel.ResponsibleDeputy is not null
            ? await _iopPersonService.GetIopPersonIdByEmail(updateModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var attributionsAgentsMappingTable = _agentsService.GetAgentsIdentifiersIdsDictionary(
            updateModel.QualifiedAttributions.Select(x => x.Agent.Identifier));

        updateModel.MapToDataset(
            publisherId,
            responsiblePersonId,
            responsiblePersonDeputyId,
            attributionsAgentsMappingTable,
            _identifierGenerator,
            entity);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dataset = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.Minimal, cancellationToken);
        await UpdateIndex(id, cancellationToken);
    }

    public async Task UpdateDatasetQualityInformation(
        Guid id,
        DatasetQualityInformationDataModel updateModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));

        var dataset = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dataset);

        EnsureDatasetQualityInformationDataModelIsValid(updateModel);

        await DeleteDatasetQualityInformation(id, cancellationToken);

        await AddDatasetQualityInformation(id, updateModel, cancellationToken);
    }

    public async Task DeleteDataset(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.Datasets.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _catalogIndexService.DeIndex(id);
    }

    public async Task DeleteDatasetQualityInformation(Guid id, CancellationToken cancellationToken = default)
    {
        // Ensure user can update entity
        var dataset = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dataset);

        await _dbContext.DatasetQualityInformationLinks
            .Where(x => x.DatasetId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.DatasetQualityInformation
            .Where(x => x.DatasetId == id)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public override async Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var allowedActions = await base.GetUserAllowActionInfo(id, cancellationToken);

        var allowVersion = await GetUserAllowVersionInfo(id, cancellationToken);

        return allowedActions.Concat([allowVersion]);
    }

    protected override async Task UpdateIndex(Guid id, CancellationToken cancellationToken)
    {
        var model = await GetDataset(id, cancellationToken);

        _catalogIndexService.UpdateIndex(model);
    }

    protected override void EnsureUserCanDeleteEntity(Dataset entity)
    {
        base.EnsureUserCanDeleteEntity(entity);

        var anyDataServicesRelationFound = _dbContext.DataServiceDatasetRelations
            .Any(x => x.DatasetId == entity.Id);

        var anyPublicServiceRelationFound = _dbContext.PublicServiceDescribedAtDatasetRelations
            .Any(x => x.IsDescribedAtId == entity.Id);

        if (anyDataServicesRelationFound || anyPublicServiceRelationFound)
        {
            throw new MethodNotAllowedException(
                $"The resource cannot be deleted. " +
                $"It is referenced from other resources.", AllowActionMessageCode.ResourceReferenced);
        }

        var isPreviousVersion = _dbContext.Datasets.Any(x => x.PreviousVersionId == entity.Id);

        if (isPreviousVersion)
        {
            throw new MethodNotAllowedException(
                $"The resource cannot be deleted. " +
                $"It is referenced as a previous version from other dataset.", AllowActionMessageCode.ResourceIsPreviousVersion);
        }
    }

    protected override async Task<Dataset> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        var entity = await base.GetEnsuredEntity(id, asNoTracking, entityIncludeLevel, cancellationToken);

        // Ensure security: only users with token can access this property
        if (!_userContextService.IsUserTokenValid())
        {
            MaskInternalProperties([entity]);
        }

        return entity;
    }

    protected override IQueryable<Dataset> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<Dataset, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = asNoTracking
            ? _dbContext.Datasets.AsNoTracking()
            : _dbContext.Datasets.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, userHasValidToken),
            _ => query.Include(d => d.Publisher),
        };

        AppendUserReadAuthorizationConditionToDatabaseQuery(ref query);
        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<Dataset> buildEntityIncludeLevelAllQuery(IQueryable<Dataset> query, bool userHasValidToken)
        {
            query = query
                .Include(d => d.ConformsTo)
                .Include(d => d.ContactPoint)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.AccessServices)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.AccessUrl)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Checksum)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.ConformsTo)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Coverage)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Documentation)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.DownloadUrl)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Image)
                .Include(d => d.Documentation)
                .Include(d => d.Image)
                .Include(d => d.IsReferencedBy)
                .Include(d => d.Keyword)
                .Include(d => d.LandingPage)
                .Include(d => d.Publisher)
                .Include(d => d.QualifiedAttribution)
                    .ThenInclude(a => a.Agent)
                .Include(d => d.QualifiedRelation)
                    .ThenInclude(r => r.Relation)
                .Include(d => d.Relation)
                .Include(d => d.TemporalCoverage)
                .AsSplitQuery();

            if (userHasValidToken)
            {
                query = query
                    .Include(d => d.ResponsiblePerson)
                    .Include(d => d.ResponsibleDeputy);
            }

            return query;
        }
    }

    private static void MaskInternalProperties(IEnumerable<Dataset> datasets)
    {
        foreach (Dataset dataset in datasets)
        {
            dataset.DataOwner = null;
        }
    }

    private void EnsureInputModelIsValid(DcatDatasetInputModel model, Guid? updateId = null)
    {
        var context = new ValidationContext<DcatDatasetInputModel>(model);

        if (updateId.HasValue)
        {
            context.RootContextData.Add(ValidationContextDataKeys.IdKey, updateId);
        }

        var result = _datasetInputModelValidator.Validate(context);

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

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private void EnsureDatasetQualityInformationDataModelIsValid(DatasetQualityInformationDataModel model)
    {
        var context = new ValidationContext<DatasetQualityInformationDataModel>(model);

        var result = _datasetQualityInformationDataModelValidator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}