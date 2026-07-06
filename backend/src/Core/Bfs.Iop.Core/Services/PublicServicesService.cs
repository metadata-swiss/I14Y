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

internal sealed class PublicServicesService : PublishableEntityServiceBase<PublicService>, IPublicServicesService
{
    private readonly IAgentsService _agentsService;
    private readonly IIopPersonsService _iopPersonService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IValidator<PublicServiceInputModel> _publicServiceInputModelValidator;

    public PublicServicesService(
        IAgentsService agentsService,
        IIopPersonsService iopPersonsService,
        IVocabulariesService vocabulariesService,
        IValidator<PublicServiceInputModel> publicServiceInputModelValidator,
        IopDbContext dbContext,
        ICatalogIndexService catalogIndexService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        IIdentifierGenerator identifierGenerator,
        IUserContextService userContextService) : base(
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

        _publicServiceInputModelValidator = publicServiceInputModelValidator 
            ?? throw new ArgumentNullException(nameof(publicServiceInputModelValidator));

        _vocabulariesService = vocabulariesService 
            ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }

    public async Task<PublicServiceModel> GetPublicService(Guid id, CancellationToken cancellationToken)
    {
        var publicService = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.All , cancellationToken);

        return publicService.MapToPublicServiceModel(_vocabulariesService);
    }

    public async Task<PublicServiceModel> GetPublicServiceByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        var id = (await _dbContext.PublicServices.SingleOrDefaultAsync(x => x.Identifiers.Contains(identifier), cancellationToken))?.Id ??
            throw new NotFoundException("No resource has been found.");

        return await GetPublicService(id, cancellationToken);
    }

    public async Task<PagedResult<PublicServiceModel>> GetPublicServicesByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));

        var query = CreateGetAuthorizedEntitiesQuery(x => ids.Contains(x.Id), asNoTracking: true, EntityIncludeLevel.All);

        var results = await query
            .ToListAsync(cancellationToken);

        return new PagedResult<PublicServiceModel>()
        {
            Page = 1,
            PageSize = int.MaxValue,
            Results = results.Select(x => x.MapToPublicServiceModel(_vocabulariesService)),
            TotalCount = results.Count,
        };
    }

    public async Task<PagedResult<PublicServiceModel>> GetPublicServices(
        string? publicServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<PublicService, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(publicServiceIdentifier) || x.Identifiers.Contains(publicServiceIdentifier)) &&
            (string.IsNullOrWhiteSpace(publisherIdentifier) || x.Publisher.Identifier == publisherIdentifier) &&
            (!publicationLevel.HasValue || x.PublicationLevel == publicationLevel.Value) &&
            (!registrationStatus.HasValue || x.RegistrationStatus == registrationStatus.Value);

        var query = CreateGetAuthorizedEntitiesQuery(filter, asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PublicServiceModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToPublicServiceModel(_vocabulariesService)),
            TotalCount = totalResults
        };
    }

    public async Task<ChannelModel> GetChannelByIdentifier(string identifier, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        var channelEntity = await _dbContext.Channels
            .Include(x => x.OwnedBy)
                .ThenInclude(o => o.OwnedBy)
            .SingleOrDefaultAsync(x => x.Identifier == identifier, cancellationToken) ??
            throw new NotFoundException("No resource has been found");

        // check authorization
        _ = await GetEnsuredEntity(channelEntity.PublicServiceId, asNoTracking: true, cancellationToken: cancellationToken);

        return channelEntity.MapToChannelModel(_vocabulariesService);
    }

    public async IAsyncEnumerable<IEnumerable<PublicServiceModel>> GetPublicServicesForIndexInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PublicServices.AsNoTracking();

        query = query
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson);

        var batch = new List<PublicServiceModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in query.ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(item.MapToPublicServiceModel(_vocabulariesService));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<PublicServiceModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<Guid> AddPublicService(PublicServiceInputModel inputModel, CancellationToken cancellationToken = default)
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

        var agentsMappingTable = _agentsService.GetAgentsIdentifiersIdsDictionary(
            inputModel.Channels
                .SelectMany(x => x.OwnedBy)
                .Select(x => x.Identifier)
                .Distinct());

        var publicService = inputModel.MapToPublicService(
            agentsMappingTable,
            publisherId,
            responsiblePersonId,
            responsiblePersonDeputyId,
            _identifierGenerator);

        await _dbContext.PublicServices.AddAsync(publicService, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await UpdateIndex(publicService.Id, cancellationToken);

        return publicService.Id;
    }

    public async Task UpdatePublicService(
        Guid id, 
        PublicServiceInputModel updateModel, 
        CancellationToken cancellationToken = default)
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

        var agentsMappingTable = _agentsService.GetAgentsIdentifiersIdsDictionary(
            updateModel.Channels
                .SelectMany(x => x.OwnedBy)
                .Select(x => x.Identifier)
                .Distinct());

        updateModel.MapToPublicService(
            agentsMappingTable,
            publisherId, 
            responsiblePersonId,
            responsiblePersonDeputyId,
            _identifierGenerator,
            entity);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(id, cancellationToken);
    }

    public async Task DeletePublicService(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.PublicServices.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _catalogIndexService.DeIndex(id);
    }

    protected override async Task UpdateIndex(Guid id, CancellationToken cancellationToken)
    {
        var model = await GetPublicService(id, cancellationToken: cancellationToken);

        _catalogIndexService.UpdateIndex(model);
    }

    protected override IQueryable<PublicService> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<PublicService, bool>>? filter,
        bool asNoTracking, 
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = asNoTracking
           ? _dbContext.PublicServices.AsNoTracking()
           : _dbContext.PublicServices.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, userHasValidToken),
            _ => query.Include(p => p.Publisher)
        };

        AppendUserReadAuthorizationConditionToDatabaseQuery(ref query);
        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<PublicService> buildEntityIncludeLevelAllQuery(IQueryable<PublicService> query, bool userHasValidToken)
        {
            query = query
                .Include(d => d.Channels)
                    .ThenInclude(c => c.OwnedBy)
                        .ThenInclude(o => o.OwnedBy)
                .Include(d => d.IsDescribedAt)
                    .ThenInclude(i => i.IsDescribedAt)
                .Include(d => d.Keyword)
                .Include(d => d.Publisher)
                .Include(d => d.Relation)
                .Include(d => d.Requires)
                    .ThenInclude(r => r.Requires)
                .Include(d => d.Relation)
                    .ThenInclude(r => r.Relation)
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

    private void EnsureInputModelIsValid(PublicServiceInputModel model, Guid? updateId = null)
    {
        var context = new ValidationContext<PublicServiceInputModel>(model);

        if (updateId.HasValue)
        {
            context.RootContextData.Add(ValidationContextDataKeys.IdKey, updateId);
        }

        var result = _publicServiceInputModelValidator.Validate(context);

        // Ensure user has access to the datasets in IsDescribedAt
        for (int i = 0; i < model.IsDescribedAt.Count(); i++)
        {
            var item = model.IsDescribedAt.ElementAt(i);
            var dataset = _dbContext.Datasets
                .Where(x => x.Id == item.Id)
                .Include(x => x.Publisher)
                .SingleOrDefault();

            var propertyName = $"{nameof(model.IsDescribedAt)}[{i}]";

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

        // Ensure user has access to all PublicServices in Requires
        for (var i = 0; i < model.Requires.Count(); i++)
        {
            var item = model.Requires.ElementAt(i);
            var propertyName = $"{nameof(model.Requires)}[{i}]";

            try
            {
                _ = GetEnsuredEntity(item.Id).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            when (ex is NotFoundException or UnauthorizedException or ForbiddenException)
            {
                result.Errors.Add(new(propertyName, ex.Message));
            }
        }

        // Ensure user has access to all PublicServices in Relations
        for (var i = 0; i < model.Relations.Count(); i++)
        {
            var item = model.Relations.ElementAt(i);
            var propertyName = $"{nameof(model.Relations)}[{i}]";

            try
            {
                _ = GetEnsuredEntity(item.Id).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            when (ex is NotFoundException or UnauthorizedException or ForbiddenException)
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
