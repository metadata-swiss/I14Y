using AngleSharp.Dom;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Validation.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

internal sealed class MappingTablesService : PublishableEntityServiceBase<MappingTable>, IMappingTablesService
{
    private readonly IAgentsService _agentsService;
    private readonly IIopPersonsService _iopPersonsService;
    private readonly IVocabulariesService _vocabulariesService;

    private readonly IValidator<MappingTableInputModel> _mappingTableValidator;
    private readonly IValidator<IEnumerable<MappingRelationInputModel>> _mappingRelationsValidator;

    public MappingTablesService(
        IAgentsService agentsService,
        IIopPersonsService iopPersonsService,
        IVocabulariesService vocabulariesService,
        IValidator<MappingTableInputModel> mappingTableValidator,
        IValidator<IEnumerable<MappingRelationInputModel>> mappingRelationsValidator,
        IopDbContext dbContext, 
        ICatalogIndexService catalogIndexService,
        IPublicationLevelPolicyService publicationLevelPolicyService, 
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        IIdentifierGenerator identifierGenerator,
        IUserContextService userContextService) : 
        base(
            dbContext, 
            catalogIndexService,
            publicationLevelPolicyService, 
            registrationStatusPolicyService, 
            authorizationService,
            identifierGenerator,
            userContextService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _iopPersonsService = iopPersonsService ?? throw new ArgumentNullException(nameof(iopPersonsService));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
        _mappingTableValidator = mappingTableValidator ?? throw new ArgumentNullException(nameof(mappingTableValidator));
        _mappingRelationsValidator = mappingRelationsValidator ?? throw new ArgumentNullException(nameof(mappingRelationsValidator));
    }

    public async Task<MappingTableModel> GetMappingTable(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.All, cancellationToken);

        var sourceConcept = TryGetAuthorizedConcept(entity.SourceUri);

        var targetConcept = TryGetAuthorizedConcept(entity.TargetUri);

        return entity.MapToMappingTableModel(
            _vocabulariesService,
            sourceConcept?.Name,
            targetConcept?.Name);
    }

    public async Task<PagedResult<MappingTableModel>> GetMappingTables(
        string? mappingTableIdentifier,
        string? publisherIdentifier,
        string? version,
        string? codeSystemUri,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<MappingTable, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(mappingTableIdentifier) || x.Identifiers.Contains(mappingTableIdentifier)) &&
            (string.IsNullOrWhiteSpace(publisherIdentifier) || x.Publisher.Identifier == publisherIdentifier) &&
            (string.IsNullOrWhiteSpace(version) || x.Version == version) &&
            (string.IsNullOrWhiteSpace(codeSystemUri) || x.SourceUri == codeSystemUri || x.TargetUri == codeSystemUri) &&
            (!publicationLevel.HasValue || x.PublicationLevel == publicationLevel) &&
            (!registrationStatus.HasValue || x.RegistrationStatus == registrationStatus);

        var query = CreateGetAuthorizedEntitiesQuery(filter, asNoTracking: true, EntityIncludeLevel.All);

        var totalResults = await query.CountAsync(cancellationToken);

        var results = await query
            .AsAsyncEnumerable()
            .OrderBy(x => x.Identifiers.First())
            .ThenByDescending(x => Version.Parse(x.Version))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var uniqueUris = results
            .Select(x => x.SourceUri)
            .Concat(results.Select(x => x.TargetUri))
            .Distinct();

        var conceptsByUris = uniqueUris.ToDictionary(
            uri => uri,
            uri => TryGetAuthorizedConcept(uri));

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToMappingTableModel(
                _vocabulariesService,
                conceptsByUris[x.SourceUri]?.Name,
                conceptsByUris[x.TargetUri]?.Name))
                .ToList(),
            TotalCount = totalResults
        };
    }


    public async IAsyncEnumerable<IEnumerable<MappingTableModel>> GetMappingTablesForIndexInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MappingTables.AsNoTracking();

        query = query
            .Include(d => d.Keywords)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson);

        var batch = new List<MappingTableModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in query.ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(item.MapToMappingTableModel(_vocabulariesService));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<MappingTableModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<IdentifierVersionExistsModel> GetIdentifierVersionExists(
        string identifier,
        string version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));
        ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

        var tables = await _dbContext.MappingTables
            .Where(x => x.Identifiers.Contains(identifier))
            .Include(x => x.Publisher)
            .ToListAsync(cancellationToken);

        return new()
        {
            IdentifierExists = tables.Count > 0,
            Publisher = tables.FirstOrDefault()?.Publisher.MapToAgentModel(_vocabulariesService),
            VersionExists = tables.Any(x => x.Version == version)
        };
    }

    public async Task<IReadOnlyDictionary<string, int>> GetReferenceCountByConceptIrisBatch(
        IReadOnlyCollection<string> conceptIris,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conceptIris, nameof(conceptIris));

        var iris = conceptIris.Distinct().ToList();
        if (iris.Count == 0)
        {
            return new Dictionary<string, int>().AsReadOnly();
        }

        // Global count, deliberately not filtered by AppendUserReadAuthorizationConditionToDatabaseQuery:
        // this feeds search ranking (a viewer-independent signal), not an authorization-scoped detail page.
        var matches = await _dbContext.MappingTables
            .Where(mt => iris.Contains(mt.SourceUri) || iris.Contains(mt.TargetUri))
            .Select(mt => new { mt.SourceUri, mt.TargetUri })
            .ToListAsync(cancellationToken);

        return iris
            .ToDictionary(iri => iri, iri => matches.Count(m => m.SourceUri == iri || m.TargetUri == iri))
            .AsReadOnly();
    }

    public async Task<MappingRelationModel> GetMappingRelation(
        Guid mappingTableId,
        Guid mappingRelationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredMappingRelationEntity(mappingTableId, mappingRelationId, asNoTracking: true, cancellationToken);

        var mappingTable = await GetEnsuredEntity(mappingTableId, asNoTracking: true, cancellationToken: cancellationToken);

        var sourceConcept = TryGetAuthorizedConcept(mappingTable.SourceUri);

        var sourceCodeListEntry = sourceConcept is not null && IriHelper.TryExtractCodeFromConceptCodeIri(entity.SourceCodeUri, out var sourceCode)
            ? await _dbContext.CodeListEntries.SingleOrDefaultAsync(x => x.IopConceptId == sourceConcept.Id && x.Code == sourceCode, cancellationToken)
            : null;

        var targetConcept = TryGetAuthorizedConcept(mappingTable.TargetUri);

        var targetCodeListEntry = targetConcept is not null && IriHelper.TryExtractCodeFromConceptCodeIri(entity.TargetCodeUri, out var targetCode)
            ? await _dbContext.CodeListEntries.SingleOrDefaultAsync(x => x.IopConceptId == targetConcept.Id && x.Code == targetCode, cancellationToken)
            : null;

        return entity.MapToMappingRelationModel(
            _vocabulariesService,
            sourceCodeListEntry,
            targetCodeListEntry);
    }

    public async Task<PagedResult<MappingRelationModel>> GetMappingRelations(
        Guid mappingTableId, 
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        // Ensure that the user can read it
        var mappingTable = await GetEnsuredEntity(mappingTableId, asNoTracking: true, cancellationToken: cancellationToken);

        var query = CreateGetMappingRelationsQueryWithFilter(asNoTracking: true, x => x.MappingTableId == mappingTableId);

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var sourceConcept = TryGetAuthorizedConcept(mappingTable.SourceUri);

        var sourceCodeListEntries = sourceConcept is not null
            ? await _dbContext.CodeListEntries.Where(x => x.IopConceptId == sourceConcept.Id).ToListAsync(cancellationToken)
            : [];

        var targetConcept = TryGetAuthorizedConcept(mappingTable.TargetUri);

        var targetCodeListEntries = targetConcept is not null
            ? await _dbContext.CodeListEntries.Where(x => x.IopConceptId == targetConcept.Id).ToListAsync(cancellationToken)
            : [];

        return new PagedResult<MappingRelationModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = result.Select(x =>
            {
                var sourceCodeListEntry = IriHelper.TryExtractCodeFromConceptCodeIri(x.SourceCodeUri, out var sourceCode)
                    ? sourceCodeListEntries.SingleOrDefault(y => y.Code == sourceCode)
                    : null;

                var targetCodeListEntry = IriHelper.TryExtractCodeFromConceptCodeIri(x.TargetCodeUri, out var targetCode)
                    ? targetCodeListEntries.SingleOrDefault(y => y.Code == targetCode)
                    : null;

                return x.MapToMappingRelationModel(_vocabulariesService, sourceCodeListEntry, targetCodeListEntry);
            }),
            TotalCount = totalCount
        };
    }

    public async Task<Guid> AddMappingTable(MappingTableInputModel inputModel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        EnsureUserCanCreateEntity(inputModel.Publisher.Identifier);

        EnsureInputModelIsValid(inputModel);

        var publisherId = await _agentsService.GetAgentId(inputModel.Publisher.Identifier, cancellationToken);

        Guid? responsibleDeputyId = inputModel.ResponsibleDeputy is not null
            ? await _iopPersonsService.GetIopPersonIdByEmail(inputModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var responsiblePersonId = await _iopPersonsService.GetIopPersonIdByEmail(
            inputModel.ResponsiblePerson.Email,
            cancellationToken);

        var entity = inputModel.MapToMappingTable(publisherId, responsiblePersonId, responsibleDeputyId, _identifierGenerator);

        await _dbContext.MappingTables.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(entity.Id, cancellationToken);

        return entity.Id;
    }

    public async Task<Guid> AddMappingTableVersion(Guid previousId, MappingTableInputModel inputModel, CancellationToken cancellationToken)
    {
        var existingEntity = await GetEnsuredEntity(previousId, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureInputModelForNewVersionIsValid(inputModel, existingEntity);

        var id = await AddMappingTable(inputModel, cancellationToken);

        var relations = await GetMappingRelations(previousId, page: 1, pageSize: int.MaxValue, cancellationToken);

        if (relations.Results.Any())
        {
            await _dbContext.AddRangeAsync(
                relations.Results.Select(x => x.MapToMappingRelationInputModel().MapToMappingRelation(id)).ToList(), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return id;
    }

    public async Task<IEnumerable<Guid>> AddRelations(
        Guid mappingTableId,
        IEnumerable<MappingRelationInputModel> inputModels,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));

        var mappingTableEntity = await GetEnsuredEntity(mappingTableId, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(mappingTableEntity);

        EnsureInputModelsAreValid(inputModels);

        var relationsEntities = inputModels.Select(x => x.MapToMappingRelation(mappingTableId)).ToList();

        await _dbContext.MappingRelations.AddRangeAsync(relationsEntities, cancellationToken);
        _dbContext.SetMainEntityStateToModified(mappingTableEntity);
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return relationsEntities.Select(x => x.Id);
    }

    public async Task UpdateMappingTable(
        Guid id,
        MappingTableInputModel updateModel, 
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureInputModelIsValid(updateModel, id);

        EnsureFirstIdentifierIsUnchanged(entity.Identifiers.FirstOrDefault(), updateModel.Identifiers.FirstOrDefault(), entity.PublicationLevel);

        var publisherId = await _agentsService.GetAgentId(updateModel.Publisher.Identifier, cancellationToken);

        Guid? responsibleDeputyId = updateModel.ResponsibleDeputy is not null
            ? await _iopPersonsService.GetIopPersonIdByEmail(updateModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var responsiblePersonId = await _iopPersonsService.GetIopPersonIdByEmail(
            updateModel.ResponsiblePerson.Email,
            cancellationToken);

        updateModel.MapToMappingTable(publisherId, responsiblePersonId, responsibleDeputyId, _identifierGenerator, entity);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(entity.Id, cancellationToken);
    }

    public async Task UpdateMappingRelation(
        Guid mappingTableId,
        Guid mappingRelationId,
        MappingRelationInputModel updateModel,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(mappingTableId, asNoTracking: true, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureInputModelsAreValid([updateModel]);

        var relation = await GetEnsuredMappingRelationEntity(
            mappingTableId, 
            mappingRelationId, 
            asNoTracking: false, 
            cancellationToken);

        updateModel.MapToMappingRelation(mappingTableId, relation);
        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMappingTable(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.MappingTables.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _catalogIndexService.DeIndex(id);
    }

    public async Task DeleteAllMappingRelations(Guid mappingTableId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(
            mappingTableId,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        await _dbContext.MappingRelations
            .Where(x => x.MappingTableId == mappingTableId)
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMappingRelation(Guid mappingTableId, Guid mappingRelationId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(
            mappingTableId,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        await _dbContext.MappingRelations
            .Where(x => x.Id == mappingRelationId)
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public override async Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var results = (await base.GetUserAllowActionInfo(id, cancellationToken)).ToList();

        var allowVersion = await GetUserAllowVersionInfo(id, cancellationToken);
        results.Add(allowVersion);

        return results;
    }

    protected override IQueryable<MappingTable> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<MappingTable, bool>>? filter,
        bool asNoTracking, 
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = asNoTracking
            ? _dbContext.MappingTables.AsNoTracking()
            : _dbContext.MappingTables.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, userHasValidToken),
            _ => query.Include(c => c.Publisher)
        };

        AppendUserReadAuthorizationConditionToDatabaseQuery(ref query);
        return query.Where(filter);

        static IQueryable<MappingTable> buildEntityIncludeLevelAllQuery(
            IQueryable<MappingTable> query,
            bool userHasValidToken)
        {
            query = query
                .Include(c => c.ConformsTo)
                .Include(c => c.Keywords)
                .Include(c => c.Publisher);

            if (userHasValidToken)
            {
                query = query
                    .Include(c => c.ResponsibleDeputy)
                    .Include(c => c.ResponsiblePerson);
            }

            return query;
        }
    }

    protected override async Task UpdateIndex(Guid id, CancellationToken cancellationToken)
    {
        var model = await GetMappingTable(id, cancellationToken);

        _catalogIndexService.UpdateIndex(model);
    }

    private IQueryable<MappingRelation> CreateGetMappingRelationsQueryWithFilter(
        bool asNoTracking,
        Expression<Func<MappingRelation, bool>> filter)
    {
        var query = asNoTracking
           ? _dbContext.MappingRelations.AsNoTracking()
           : _dbContext.MappingRelations.AsQueryable();

        return query.Where(filter);
    }

    private async Task<MappingRelation> GetEnsuredMappingRelationEntity(
        Guid mappingTableId,
        Guid mappingRelationId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        // Ensure user can get entity
        _ = await GetEnsuredEntity(mappingTableId, asNoTracking: true, cancellationToken: cancellationToken);

        var query = CreateGetMappingRelationsQueryWithFilter(
            asNoTracking,
            x => x.MappingTableId == mappingTableId);

        var entity = await query
            .SingleOrDefaultAsync(c => c.Id == mappingRelationId, cancellationToken)
            ?? throw new NotFoundException($"No resource has been found.");

        return entity;
    }

    private void EnsureInputModelIsValid(MappingTableInputModel model, Guid? updateId = null)
    {
        var context = new ValidationContext<MappingTableInputModel>(model);

        if (updateId is not null)
        {
            context.RootContextData.Add(ValidationContextDataKeys.IdKey, updateId);
        }

        var result = _mappingTableValidator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private void EnsureInputModelsAreValid(IEnumerable<MappingRelationInputModel> models)
    {
        var context = new ValidationContext<IEnumerable<MappingRelationInputModel>>(models);

        var result = _mappingRelationsValidator.Validate(context);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private static void EnsureInputModelForNewVersionIsValid(MappingTableInputModel inputModel, MappingTable entity)
    {
        var result = new ValidationResult();

        if (inputModel.Identifiers.Count() != entity.Identifiers.Length ||
            entity.Identifiers.Except(inputModel.Identifiers).Any())
        {
            result.Errors.Add(
                new(
                    nameof(inputModel.Identifiers),
                    "The new version must have the same identifiers."));
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    private IopConcept? TryGetAuthorizedConcept(string uri)
    {
        IopConcept? concept = null;

        if (IriHelper.TryExtractConceptIdentifierAndVersion(
                uri,
                out var conceptIdentifier,
                out var conceptVersion))
        {
            concept = _dbContext.IopConcepts
                .Include(c => c.Publisher)
                .SingleOrDefault(x => x.Identifiers.Contains(conceptIdentifier) && x.Version == conceptVersion);
        }

        if (concept is not null)
        {
            try
            {
                _publishableEntityAuthorizationService.EnsureUserCanReadPublishableEntity(concept);
            }
            catch (Exception ex) when (ex is UnauthorizedException or ForbiddenException)
            {
                concept = null;
            }
        }

        return concept;
    }
}
