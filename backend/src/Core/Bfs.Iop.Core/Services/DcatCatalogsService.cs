using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

internal class DcatCatalogsService : AuthorizedEntityServiceBase<DcatCatalog>, IDcatCatalogsService
{
    private readonly IopDbContext _dbContext;
    private readonly IAgentsService _agentsService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IPublishableEntityAuthorizationService _publishableEntityAuthorizationService;
    private readonly IValidator<DcatCatalogInputModel> _dcatCatalogInputModelValidator;

    public DcatCatalogsService(
        IopDbContext dbContext,
        IAgentsService agentsService,
        IValidator<DcatCatalogInputModel> dcatCatalogInputModelValidator,
        IVocabulariesService vocabulariesService,
        IPublishableEntityAuthorizationService publishableEntityAuthorizationService,
        IEntityAuthorizationService entityAuthorizationService,
        IUserContextService userContextService) : base(entityAuthorizationService, userContextService)
    {
        _dbContext = dbContext 
            ?? throw new ArgumentNullException(nameof(dbContext));

        _dcatCatalogInputModelValidator = dcatCatalogInputModelValidator 
            ?? throw new ArgumentNullException(nameof(dcatCatalogInputModelValidator));

        _vocabulariesService = vocabulariesService 
            ?? throw new ArgumentNullException(nameof(vocabulariesService));

        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));

        _publishableEntityAuthorizationService = publishableEntityAuthorizationService;
    }

    public async Task<DcatCatalogModel> GetDcatCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        // For now, no authorization check is made to read DcatCatalogs

        var query = CreateGetEntitiesQuery(x => x.Id == id, asNoTracking: true, entityIncludeLevel: EntityIncludeLevel.All);

        var entity = await query.SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return entity is null
            ? throw new NotFoundException("No resource has been found.")
            : entity.MapToDcatCatalogModel(_vocabulariesService);
    }

    public async Task<PagedResult<DcatCatalogModel>> GetDcatCatalogs(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        // For now, no authorization check is made to read DcatCatalogs
        var query = CreateGetEntitiesQuery(filter: null, asNoTracking: true, EntityIncludeLevel.Minimal);

        var count = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DcatCatalogModel>()
        {
            Page = page,
            PageSize = pageSize,
            Results = results.Select(x => x.MapToDcatCatalogModel(_vocabulariesService)),
            TotalCount = count
        };
    }

    public async Task<PagedResult<DcatCatalogModel>> GetDcatCatalogsFromPublishers(
        IEnumerable<string> publisherIdentifiers,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publisherIdentifiers, nameof(publisherIdentifiers));

        var ids = (await _agentsService.GetAgents(publisherIdentifiers, cancellationToken)).Select(x => x.Id);

        var query = CreateGetAuthorizedEntitiesQuery(x => ids.Contains(x.PublisherId), asNoTracking: true, EntityIncludeLevel.Minimal);

        var totalCount = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DcatCatalogModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = results.Select(x => x.MapToDcatCatalogModel(_vocabulariesService)),
            TotalCount = totalCount
        };
    }

    public async Task<Guid> AddDcatCatalog(DcatCatalogInputModel inputModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        EnsureUserCanCreateEntity(inputModel.Publisher.Identifier);

        EnsureInputModelIsValid(inputModel);

        var publisherId = await _agentsService.GetAgentId(inputModel.Publisher.Identifier, cancellationToken);

        var entity = inputModel.MapToDcatCatalog(publisherId);

        await _dbContext.DcatCatalogs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateDcatCatalog(Guid id, DcatCatalogInputModel updateModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));

        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureInputModelIsValid(updateModel);

        var publisherId = await _agentsService.GetAgentId(updateModel.Publisher.Identifier, cancellationToken);

        updateModel.MapToDcatCatalog(publisherId, entity);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDcatCatalog(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.DcatCatalogs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<DcatCatalogRecordModel>> GetDcatCatalogRecords(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = CreateGetDcatCatalogRecordsEntitiesQuery(asNoTracking: true, EntityIncludeLevel.All, x => x.DcatCatalogId == id);

        var totalCount = await query.CountAsync(cancellationToken);
        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = results.Select(x => x.MapToDcatCatalogRecordModel(_vocabulariesService)),
            TotalCount = totalCount,
        };
    }

    public async Task<DcatCatalogRecordModel> GetDcatCatalogRecord(
        Guid id, 
        Guid dcatCatalogRecordId, 
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredDcatCatalogRecord(
            id, 
            dcatCatalogRecordId, 
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken: cancellationToken);

        return entity.MapToDcatCatalogRecordModel(_vocabulariesService);
    }

    public async Task<PagedResult<DcatCatalogRecordModel>> GetDcatCatalogRecordsFromResource(
        Guid resourceId, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = CreateGetDcatCatalogRecordsEntitiesQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            x => x.PrimaryTopic.ResourceId == resourceId);

        var totalCount = await query.CountAsync(cancellationToken);
        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = results.Select(x => x.MapToDcatCatalogRecordModel(_vocabulariesService)),
            TotalCount = totalCount,
        };
    }

    public async Task<Guid> AddDcatCatalogRecord(Guid id, DcatCatalogRecordInputModel inputModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        var dcatCatalog = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dcatCatalog);

        EnsureInputModelIsValid(inputModel, dcatCatalog.MapToDcatCatalogModel(_vocabulariesService));

        var entity = inputModel.MapToDcatCatalogRecord(id);

        await _dbContext.DcatCatalogRecords.AddAsync(entity, cancellationToken);
        _dbContext.SetMainEntityStateToModified(dcatCatalog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateDcatCatalogRecord(Guid id, Guid dcatCatalogRecordId, DcatCatalogRecordInputModel updateModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));
        
        var dcatCatalog = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dcatCatalog);

        EnsureInputModelIsValid(updateModel, dcatCatalog.MapToDcatCatalogModel(_vocabulariesService), dcatCatalogRecordId);

        var entity = await GetEnsuredDcatCatalogRecord(id, dcatCatalogRecordId, asNoTracking: false, cancellationToken: cancellationToken);

        updateModel.MapToDcatCatalogRecord(id, entity);

        _dbContext.SetMainEntityStateToModified(dcatCatalog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDcatCatalogRecord(Guid id, Guid dcatCatalogRecordId, CancellationToken cancellationToken = default)
    {
        var dcatCatalog = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(dcatCatalog);

        var entity = await GetEnsuredDcatCatalogRecord(id, dcatCatalogRecordId, asNoTracking: false, cancellationToken: cancellationToken);

        _dbContext.DcatCatalogRecords.Remove(entity);
        _dbContext.SetMainEntityStateToModified(dcatCatalog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override async Task<DcatCatalog> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = CreateGetAuthorizedEntitiesQuery(x => x.Id == id, asNoTracking, entityIncludeLevel);

        var entity = await query.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (entity is null)
        {
            throw (await _dbContext.FindAsync<DcatCatalog>(id, cancellationToken)) is null
                ? new NotFoundException("No resource has been found.")
                : userHasValidToken
                    ? new ForbiddenException("The resource is forbidden.")
                    : new UnauthorizedException("No authorization to access the resource.");
        }

        return entity;
    }

    private async Task<DcatCatalogRecord> GetEnsuredDcatCatalogRecord(
        Guid dcatCatalogId,
        Guid dcatCatalogRecordId,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        // Ensure user can get entity
        _ = await GetEnsuredEntity(dcatCatalogId, asNoTracking: true, cancellationToken: cancellationToken);

        var query = CreateGetDcatCatalogRecordsEntitiesQuery(asNoTracking, entityIncludeLevel, x => x.DcatCatalogId == dcatCatalogId);

        var entity = await query
            .SingleOrDefaultAsync(c => c.Id == dcatCatalogRecordId, cancellationToken)
            ?? throw new NotFoundException($"No resource has been found.");

        return entity;
    }

    private IQueryable<DcatCatalog> CreateGetEntitiesQuery(
        Expression<Func<DcatCatalog, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? _dbContext.DcatCatalogs.AsNoTracking()
            : _dbContext.DcatCatalogs.AsQueryable();

        query = query
            .Include(x => x.Publisher);

        return query
            .Where(filter);
    }

    private IQueryable<DcatCatalog> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<DcatCatalog, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel)
    {
        var userBusinessRole = _userContextService.GetUserBusinessRole();

        var query = CreateGetEntitiesQuery(filter, asNoTracking, entityIncludeLevel);

        if (!(userBusinessRole is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward))
        {
            var userAgencies = _userContextService.GetUserAgencies();

            query = query.Where(x => userAgencies.Contains(x.Publisher.Identifier));
        }

        return query;
    }

    private IQueryable<DcatCatalogRecord> CreateGetDcatCatalogRecordsEntitiesQuery(
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<DcatCatalogRecord, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? _dbContext.DcatCatalogRecords.AsNoTracking()
            : _dbContext.DcatCatalogRecords.AsQueryable();

        query = query
            .Include(x => x.PrimaryTopic)
            .Include(x => x.Themes);

        return query.Where(filter);
    }

    private void EnsureInputModelIsValid(DcatCatalogInputModel inputModel)
    {
        var context = new ValidationContext<DcatCatalogInputModel>(inputModel);
        var result = _dcatCatalogInputModelValidator.Validate(context);

        // Ensure all vocabularies exist
        var allVocabularyConfigs = _vocabulariesService.GetVocabularyConfigs(default).GetAwaiter().GetResult();

        for (var i = 0; i < inputModel.ThemeTaxonomy.Count(); i++)
        {
            var item = inputModel.ThemeTaxonomy.ElementAt(i);
            var propertyName = $"{nameof(inputModel.ThemeTaxonomy)}[{i}]";

            if (allVocabularyConfigs.SingleOrDefault(x => x.VocabularyIdentifier == item) is null)
            {
                result.Errors.Add(new(propertyName, $"The string '{item}' is not a vocabulary identifier."));
            }
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private void EnsureInputModelIsValid(DcatCatalogRecordInputModel inputModel, DcatCatalogModel dcatCatalogModel, Guid? dcatCatalogRecordId = null)
    {
        var result = new ValidationResult();

        // Check if a record already exists:
        if (dcatCatalogRecordId is not null)
        {
            var query = CreateGetDcatCatalogRecordsEntitiesQuery(
                asNoTracking: false,
                EntityIncludeLevel.All,
                x => x.DcatCatalogId == dcatCatalogModel.Id && x.PrimaryTopic.ResourceId == inputModel.PrimaryTopic.ResourceId && x.Id != dcatCatalogRecordId);

            if (query.Any())
            {
                throw new ConflictException("A record for this resource already exists.");
            }
        }

        // Validate the primary topic:
        var primaryTopic = tryFindPrimaryTopic(inputModel.PrimaryTopic);

        if (primaryTopic is null)
        {
            result.Errors.Add(new(nameof(inputModel.PrimaryTopic), "No resource has been found."));
        }
        else if (primaryTopic.PublisherId != dcatCatalogModel.Publisher.Id)
        {
            result.Errors.Add(new(nameof(inputModel.PrimaryTopic), "The resource and the dcat catalog must have the same publisher."));
        }

        // Validate the themes
        if (inputModel.Themes.Select(x => $"{x.ThemeTaxonomy}{x.Code}").Distinct().Count() != inputModel.Themes.Count())
        {
            result.Errors.Add(new(nameof(inputModel.Themes), "Contains duplicated codes."));
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        for (int i = 0; i < inputModel.Themes.Count(); i++)
        {
            var item = inputModel.Themes.ElementAt(i);
            var propertyName = $"{nameof(inputModel.Themes)}[{i}]";

            if (!dcatCatalogModel.ThemeTaxonomy.Contains(item.ThemeTaxonomy))
            {
                result.Errors.Add(new(propertyName, $"The taxonomy '{item.ThemeTaxonomy}' is not defined in the catalog."));
                continue;
            }

            var vocabulary = _vocabulariesService.GetVocabulary(item.ThemeTaxonomy, default).GetAwaiter().GetResult();

            if (!vocabulary.Entries.Any(x => x.Code == item.Code))
            {
                result.Errors.Add(new(propertyName, $"The code '{item.Code}' does not exist in vocabulary '{item.ThemeTaxonomy}'."));
            }
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        PublishableEntityBase? tryFindPrimaryTopic(DcatCatalogResourceModel primaryTopic)
        {
            return primaryTopic.ResourceType switch
            {
                DcatCatalogType.Dataset => _dbContext.Find<Dataset>(primaryTopic.ResourceId),
                DcatCatalogType.DataService => _dbContext.Find<DataService>(primaryTopic.ResourceId),
                _ => null
            };
        }
    }
}
