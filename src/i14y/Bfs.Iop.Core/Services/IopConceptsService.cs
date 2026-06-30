using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Validation.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Settings;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Services;

internal sealed class IopConceptsService : PublishableEntityServiceBase<IopConcept>, IIopConceptsService
{
    private readonly IAgentsService _agentsService;
    private readonly IIopPersonsService _iopPersonsService;
    private readonly IVocabulariesService _vocabulariesService;
    private readonly IIopConceptsValidationService _conceptsValidationService;
    private readonly ICodeListEntryIndexService _codeListEntryLuceneService;
    private readonly string _baseIriUrl;

    public IopConceptsService(
        IopDbContext dbContext,
        ICodeListEntryIndexService codeListEntryLuceneService,
        IAgentsService agentsService,
        IIopPersonsService iopPersonsService,
        IUserContextService userContextService,
        IVocabulariesService vocabulariesService,
        IIopConceptsValidationService conceptsValidationService,
        IPublicationLevelPolicyService publicationLevelPolicyService,
        IRegistrationStatusPolicyService registrationStatusPolicyService,
        IPublishableEntityAuthorizationService authorizationService,
        ICatalogIndexService catalogIndexService,
        IIdentifierGenerator identifierGenerator,
        IOptions<I14YOptions> i14yOptions) : base(
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
        _conceptsValidationService = conceptsValidationService ?? throw new ArgumentNullException(nameof(conceptsValidationService));
        _codeListEntryLuceneService = codeListEntryLuceneService ?? throw new ArgumentNullException(nameof(codeListEntryLuceneService));
        ArgumentNullException.ThrowIfNull(i14yOptions, nameof(i14yOptions));
        _baseIriUrl = i14yOptions.Value.IriBaseUrl.TrimEnd('/');
    }

    public async Task<IopConceptModel> GetIopConcept(
        Guid id,
        bool includeCodeListEntries,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(
            id,
            asNoTracking: true,
            EntityIncludeLevel.All,
            cancellationToken);

        if (includeCodeListEntries && entity.ConceptType is ConceptType.CodeList)
        {
            entity.CodeListEntries = (await GetEnsuredSortedCodeListEntryEntities(
                entity,
                asNoTracking: true,
                entityIncludeLevel: EntityIncludeLevel.All,
                sortProperty: entity.CodeListEntryDefaultSortProperty ?? default,
                cancellationToken: cancellationToken)).Results.ToList();
        }

        var replaces = await ResolveReplaces(entity, cancellationToken);

        var isReplacedBy = await GetIsReplacedBy(entity, cancellationToken);

        return entity.MapToIopConceptModel(_vocabulariesService, replaces, isReplacedBy);
    }

    public async Task<PagedResult<IopConceptModel>> GetIopConcepts(
        string? conceptIdentifier,
        string? publisherIdentifier,
        string? version,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<IopConcept, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(conceptIdentifier) || x.Identifiers.Contains(conceptIdentifier)) &&
            (string.IsNullOrWhiteSpace(publisherIdentifier) || x.Publisher.Identifier == publisherIdentifier) &&
            (string.IsNullOrWhiteSpace(version) || x.Version == version) &&
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

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToIopConceptModel(_vocabulariesService)),
            TotalCount = totalResults
        };
    }

    public async Task<IdentifierVersionExistsModel> GetIdentifierVersionExists(
        string identifier,
        string version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));
        ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

        var concepts = await _dbContext.IopConcepts
            .Where(x => x.Identifiers.Contains(identifier))
            .Include(x => x.Publisher)
            .ToListAsync(cancellationToken);

        return new()
        {
            IdentifierExists = concepts.Count > 0,
            Publisher = concepts.FirstOrDefault()?.Publisher.MapToAgentModel(_vocabulariesService),
            VersionExists = concepts.Any(x => x.Version == version)
        };
    }

    public async IAsyncEnumerable<IEnumerable<IopConceptModel>> GetIopConceptsForIndexInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.IopConcepts.AsNoTracking();

        query = query
            .Include(d => d.Keywords)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson);

        var batch = new List<IopConceptModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in query.ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(item.MapToIopConceptModel(_vocabulariesService));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<IopConceptModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<CodeListEntryModel> GetCodeListEntry(
        Guid conceptId,
        Guid codeListEntryId,
        CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredCodeListEntryEntity(
            conceptId,
            codeListEntryId,
            asNoTracking: true,
            EntityIncludeLevel.All,
            cancellationToken);

        return entity.MapToCodeListEntryModel();
    }

    public async Task<CodeListEntryModel> GetCodeListEntryByCode(
        Guid conceptId,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));

        var codeListEntryId = await GetCodeListEntryIdFromCode(conceptId, code, cancellationToken);

        return await GetCodeListEntry(conceptId, codeListEntryId, cancellationToken);
    }

    public async Task<PagedResult<CodeListEntryModel>> GetCodeListEntries(
        Guid conceptId,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        sortProperty?.EnsureValueIsValid();
        sortOrder.EnsureValueIsValid();
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        var entity = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var result = await GetEnsuredSortedCodeListEntryEntities(
            entity,
            filter: x => true,
            sortProperty,
            sortOrder,
            page,
            pageSize,
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken);

        return new()
        {
            Page = result.Page,
            PageSize = pageSize is int.MaxValue ? result.TotalCount : pageSize,
            Results = result.Results.Select(x => x.MapToCodeListEntryModel()),
            TotalCount = result.TotalCount,
        };
    }

    public async Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesByRoot(
        Guid conceptId,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        sortProperty?.EnsureValueIsValid();
        sortOrder.EnsureValueIsValid();
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        var entity = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var result = await GetEnsuredSortedCodeListEntryEntities(
            entity,
            filter: x => x.ParentCodeListEntryId == null,
            sortProperty,
            sortOrder,
            page,
            pageSize,
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken);

        return new()
        {
            Page = result.Page,
            PageSize = pageSize is int.MaxValue ? result.TotalCount : pageSize,
            Results = result.Results.Select(x => x.MapToCodeListEntryModel()),
            TotalCount = result.TotalCount,
        };
    }

    public async Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesChildrenOfParentCode(
        Guid conceptId,
        string parentCode,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        sortProperty?.EnsureValueIsValid();
        sortOrder.EnsureValueIsValid();
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        var entity = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var result = await GetEnsuredSortedCodeListEntryEntities(
            entity,
            filter: x => x.ParentCodeListEntryId.HasValue && x.ParentCodeListEntry!.Code == parentCode,
            sortProperty,
            sortOrder,
            page,
            pageSize,
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken);

        return new()
        {
            Page = result.Page,
            PageSize = pageSize is int.MaxValue ? result.TotalCount : pageSize,
            Results = result.Results.Select(x => x.MapToCodeListEntryModel()),
            TotalCount = result.TotalCount,
        };
    }

    public async Task<bool> GetCodeListEntriesHasParentCodes(Guid conceptId, CancellationToken cancellationToken = default)
    {
        _ = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var query = CreateGetConceptCodeListEntriesQueryWithFilter(
            conceptId,
            asNoTracking: true,
            EntityIncludeLevel.Minimal,
            x => x.ParentCodeListEntryId != null);

        var hasParentCodes = await query.AnyAsync(cancellationToken);

        return hasParentCodes;
    }

    public async Task<int> GetCodeListEntriesPageNumberFromSameParent(
        Guid conceptId,
        string codeValue,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int pageSize,
        CancellationToken cancellationToken)
    {
        sortProperty?.EnsureValueIsValid();
        sortOrder.EnsureValueIsValid();
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        var entity = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var codeListEntry = await GetCodeListEntryByCode(conceptId, codeValue, cancellationToken);

        Guid? parentCodeListEntryId = null;

        if (!string.IsNullOrWhiteSpace(codeListEntry.ParentCode))
        {
            var parentCodeListEntry = await GetCodeListEntryByCode(conceptId, codeListEntry.ParentCode, cancellationToken);

            parentCodeListEntryId = parentCodeListEntry.Id;
        }

        var result = await GetEnsuredSortedCodeListEntryEntities(
            entity,
            filter: x => x.ParentCodeListEntryId == parentCodeListEntryId,
            sortProperty,
            sortOrder,
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken: cancellationToken);

        var index = result.Results
            .ToList()
            .FindIndex(x => x.Code == codeValue);

        if (index == -1)
        {
            throw new NotFoundException("No resource has been found.");
        }

        var pageNumber = (int)Math.Ceiling((index + 1) / (double)pageSize);

        return pageNumber;
    }

    public async Task<PagedResult<CodeListEntryModel>> GetCodeListEntriesForAutoComplete(
        Guid conceptId,
        string codePrefix,
        CodeListEntrySortProperty? sortProperty,
        SortOrder sortOrder,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        sortProperty?.EnsureValueIsValid();
        sortOrder.EnsureValueIsValid();
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

        var entity = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var result = await GetEnsuredSortedCodeListEntryEntities(
            entity,
            filter: x => x.Code.StartsWith(codePrefix),
            sortProperty,
            sortOrder,
            page,
            pageSize,
            asNoTracking: true,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken);

        return new()
        {
            Page = result.Page,
            PageSize = result.PageSize,
            Results = result.Results.Select(x => x.MapToCodeListEntryModel()),
            TotalCount = result.TotalCount,
        };
    }

    public async IAsyncEnumerable<List<CodeListEntryModel>> GetCodeListEntriesForIndexInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.CodeListEntries.AsNoTracking();

        query = query
            .Include(c => c.Annotations);

        var batch = new List<CodeListEntryModel>(batchSize);

        await foreach (var codeListEntryEntity in query.ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (codeListEntryEntity is null)
            {
                continue;
            }

            batch.Add(codeListEntryEntity.MapToCodeListEntryModel());

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<CodeListEntryModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesByIds(IEnumerable<Guid> codeListEntryIds, CancellationToken cancellationToken = default)
    {
        var entityDict = await CreateGetCodeListEntriesQueryWithFilter(
            true,
            EntityIncludeLevel.All,
            e => codeListEntryIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken: cancellationToken);

        var orderedEntities = codeListEntryIds
            .Where(entityDict.ContainsKey)
            .Select(id => entityDict[id].MapToCodeListEntryModel())
            .ToList();

        return orderedEntities;
    }

    public async Task<IEnumerable<CodeListEntryModel>> GetCodeListEntriesByCodes(Guid conceptId, IEnumerable<string> codeListEntryCodes, CancellationToken cancellationToken = default)
    {
        var query = CreateGetConceptCodeListEntriesQueryWithFilter(
            conceptId,
            true,
            EntityIncludeLevel.All,
            x => x.IopConceptId == conceptId && codeListEntryCodes.Contains(x.Code));

        var entityDict = await query
            .ToDictionaryAsync(e => e.Code, cancellationToken: cancellationToken);

        var orderedEntities = codeListEntryCodes
            .Select(code => entityDict[code].MapToCodeListEntryModel())
            .ToList();

        return orderedEntities;
    }

    public override async Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var allowedActions = await base.GetUserAllowActionInfo(id, cancellationToken);

        var allowVersion = await GetUserAllowVersionInfo(id, cancellationToken);

        return allowedActions.Concat([allowVersion]);
    }

    public async Task<Guid> AddIopConcept(IopConceptInputModel inputModel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        EnsureUserCanCreateEntity(inputModel.Publisher.Identifier);

        _conceptsValidationService.EnsureConceptCanBeAdded(inputModel);

        var publisherId = await _agentsService.GetAgentId(inputModel.Publisher.Identifier, cancellationToken);

        Guid? responsibleDeputyId = inputModel.ResponsibleDeputy is not null
            ? await _iopPersonsService.GetIopPersonIdByEmail(inputModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var responsiblePersonId = await _iopPersonsService.GetIopPersonIdByEmail(
            inputModel.ResponsiblePerson.Email,
            cancellationToken);

        var replacesResources = await ResolveReplacesInput(inputModel.Replaces, cancellationToken);

        var entity = inputModel.MapToIopConcept(publisherId, responsiblePersonId, responsibleDeputyId, _identifierGenerator, replaces: replacesResources);

        await _dbContext.IopConcepts.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(entity.Id, cancellationToken);

        return entity.Id;
    }

    public async Task<Guid> AddIopConceptVersion(Guid previousId, IopConceptInputModel inputModel, CancellationToken cancellationToken)
    {
        await _conceptsValidationService.EnsureConceptVersionCanBeAdded(previousId, inputModel, cancellationToken);

        var existingConceptVersion = await GetEnsuredEntity(previousId, asNoTracking: true, EntityIncludeLevel.All, cancellationToken);

        var conceptId = await AddIopConcept(inputModel, cancellationToken);

        if (existingConceptVersion.ConceptType == ConceptType.CodeList)
        {
            var entries = await GetEnsuredSortedCodeListEntryEntities(
                existingConceptVersion, 
                entityIncludeLevel: EntityIncludeLevel.All,
                cancellationToken: cancellationToken);

            await _dbContext.AddRangeAsync(
                [.. entries.Results.Select(x => x.
                    MapToCodeListEntryModel().
                    MapToCodeListEntryInputModel().
                    MapToCodeListEntry(conceptId, x.Position, x.ParentCodeListEntryId))], cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return conceptId;
    }

    public async Task<IEnumerable<Guid>> AddCodeListEntries(
        Guid conceptId,
        IEnumerable<CodeListEntryInputModel> inputModels,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));

        var conceptEntity = await GetEnsuredEntity(conceptId, cancellationToken: cancellationToken);

        if (conceptEntity.ConceptType is not ConceptType.CodeList)
        {
            throw new InvalidOperationException($"The concept must be of the type '{ConceptType.CodeList}'.");
        }

        EnsureUserCanUpdateEntity(conceptEntity);

        var allCodeListEntries = (await GetEnsuredSortedCodeListEntryEntities(
                conceptEntity,
                asNoTracking: true,
                entityIncludeLevel: EntityIncludeLevel.All,
                cancellationToken: cancellationToken)).Results;

        _conceptsValidationService.EnsureCodeListEntriesCanBeAdded(
            inputModels,
            conceptEntity,
            allCodeListEntries);

        var availablePosition = allCodeListEntries.Any() 
            ? allCodeListEntries.Max(c => c.Position) + 1
            : 0;

        // Map without parent codes because the parent code may not exist in the database yet.
        var codeListEntryEntities = inputModels.Select((x, i) => x.MapToCodeListEntry(conceptId, availablePosition + i)).ToList();

        for (int i = 0; i < codeListEntryEntities.Count; i++)
        {
            var inputModel = inputModels.ElementAt(i);

            if (inputModel.ParentCode is not null)
            {
                Guid? parentCodeId = allCodeListEntries.SingleOrDefault(x => x.Code == inputModel.ParentCode)?.Id;

                parentCodeId ??= codeListEntryEntities.SingleOrDefault(x => x.Code == inputModel.ParentCode)?.Id;

                if (parentCodeId is null)
                {
                    throw new NotFoundException($"Code {inputModel.ParentCode} was not found");
                }

                codeListEntryEntities[i].ParentCodeListEntryId = parentCodeId;
            }
        }

        await _dbContext.CodeListEntries.AddRangeAsync(codeListEntryEntities, cancellationToken);
        _dbContext.SetMainEntityStateToModified(conceptEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _codeListEntryLuceneService.Index(codeListEntryEntities.Select(x => x.MapToCodeListEntryModel()));

        return codeListEntryEntities.Select(x => x.Id);
    }

    public async Task UpdateConcept(Guid id, IopConceptInputModel updateModel, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(
            id,
            asNoTracking: false,
            EntityIncludeLevel.All,
            cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        _conceptsValidationService.EnsureConceptCanBeUpdated(
            id,
            updateModel,
            entity,
            (await GetEnsuredSortedCodeListEntryEntities(
                    entity,
                    asNoTracking: true,
                    entityIncludeLevel: EntityIncludeLevel.All,
                    cancellationToken: cancellationToken)).Results
                );

        EnsureFirstIdentifierIsUnchanged(entity.Identifiers.FirstOrDefault(), updateModel.Identifiers.FirstOrDefault(), entity.PublicationLevel);

        var publisherId = await _agentsService.GetAgentId(updateModel.Publisher.Identifier, cancellationToken);

        Guid? responsibleDeputyId = updateModel.ResponsibleDeputy is not null
            ? await _iopPersonsService.GetIopPersonIdByEmail(updateModel.ResponsibleDeputy.Email, cancellationToken)
            : null;

        var responsiblePersonId = await _iopPersonsService.GetIopPersonIdByEmail(
            updateModel.ResponsiblePerson.Email,
            cancellationToken);

        var hasConceptTypeChangedFromCodeList = entity.ConceptType is ConceptType.CodeList &&
            entity.ConceptType != updateModel.ConceptType;

        if (hasConceptTypeChangedFromCodeList)
        {
            _dbContext.CodeListEntries.RemoveRange((await GetEnsuredSortedCodeListEntryEntities(entity, cancellationToken: cancellationToken)).Results);

            updateModel = updateModel with
            {
                CodeListEntryValueMaxLength = null,
                CodeListEntryValueType = null,
                CodeListEntryDefaultSortProperty = null
            };
        }

        var replacesResources = await ResolveReplacesInput(updateModel.Replaces, cancellationToken);

        updateModel.MapToIopConcept(publisherId, responsiblePersonId, responsibleDeputyId, _identifierGenerator, entity, replaces: replacesResources);

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(entity.Id, cancellationToken);
    }

    public async Task UpdateCodeListEntry(
        Guid conceptId,
        Guid codeListEntryId,
        CodeListEntryInputModel updateModel,
        CancellationToken cancellationToken = default)
    {
        var conceptEntity = await GetEnsuredEntity(
            conceptId,
            asNoTracking: false,
            EntityIncludeLevel.Minimal,
            cancellationToken);

        EnsureUserCanUpdateEntity(conceptEntity);

        _conceptsValidationService.EnsureCodeListEntryCanBeUpdated(
            codeListEntryId,
            updateModel,
            conceptEntity,
            (await GetEnsuredSortedCodeListEntryEntities(
                conceptEntity,
                asNoTracking: true,
                entityIncludeLevel: EntityIncludeLevel.All,
                cancellationToken: cancellationToken)).Results);

        var codeListEntryEntity = await GetEnsuredCodeListEntryEntity(
            conceptId,
            codeListEntryId,
            entityIncludeLevel: EntityIncludeLevel.All,
            cancellationToken: cancellationToken);

        Guid? parentCodeId = updateModel.ParentCode is not null
            ? await GetCodeListEntryIdFromCode(conceptId, updateModel.ParentCode, cancellationToken)
            : null;

        updateModel.MapToCodeListEntry(conceptId, codeListEntryEntity.Position, parentCodeId, codeListEntryEntity);

        _dbContext.SetMainEntityStateToModified(conceptEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _codeListEntryLuceneService.UpdateIndex([codeListEntryEntity.MapToCodeListEntryModel()]);
    }

    public async Task UpdateIsLocked(Guid id, bool value, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        if (value)
        {
            EnsureConceptCanBeLocked(entity);
        }
        else
        {
            EnsureConceptCanBeUnlocked(entity);
        }

        entity.IsLocked = value;
        entity.ModifiedAt = DateTimeOffset.UtcNow;

        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UpdateIndex(entity.Id, cancellationToken);
    }

    public async Task DeleteIopConcept(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.IopConcepts.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _catalogIndexService.DeIndex(id);
    }

    public async Task DeleteCodeListEntry(Guid conceptId, Guid codeListEntryId, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(
            conceptId,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        var codeListEntryEntity = await GetEnsuredCodeListEntryEntity(
            conceptId,
            codeListEntryId,
            cancellationToken: cancellationToken);

        _dbContext.CodeListEntries.Remove(codeListEntryEntity);
        _dbContext.SetMainEntityStateToModified(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _codeListEntryLuceneService.DeIndex([codeListEntryEntity.Id]);
    }

    public async Task DeleteAllCodeListEntriesFromIopConcept(Guid conceptId, CancellationToken cancellationToken)
    {
        var conceptEntity = await GetEnsuredEntity(conceptId, cancellationToken: cancellationToken);

        if (conceptEntity.ConceptType is not ConceptType.CodeList)
        {
            throw new InvalidOperationException($"The concept must be of the type '{ConceptType.CodeList}'.");
        }

        EnsureUserCanUpdateEntity(conceptEntity);

        var codeListEntriesIds = _dbContext.CodeListEntries
            .Where(c => c.IopConceptId == conceptId)
            .Select(x => x.Id)
            .ToHashSet();

        await _dbContext.CodeListEntries.Where(x => x.IopConceptId == conceptId).ExecuteDeleteAsync(cancellationToken);

        _dbContext.SetMainEntityStateToModified(conceptEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _codeListEntryLuceneService.DeIndex(codeListEntriesIds);
    }

    protected override async Task UpdateIndex(Guid id, CancellationToken cancellationToken)
    {
        var model = await GetIopConcept(id, false, cancellationToken);

        _catalogIndexService.UpdateIndex(model);
    }

    protected override void EnsureUserCanUpdateEntity(IopConcept entity)
    {
        if (entity.IsLocked)
        {
            throw new MethodNotAllowedException($"The resource is locked.", AllowActionMessageCode.ResourceIsLocked);
        }

        base.EnsureUserCanUpdateEntity(entity);
    }

    protected override void EnsureUserCanDeleteEntity(IopConcept entity)
    {
        if (entity.IsLocked)
        {
            throw new MethodNotAllowedException($"The resource is locked.", AllowActionMessageCode.ResourceIsLocked);
        }

        base.EnsureUserCanDeleteEntity(entity);
    }

    private void EnsureConceptCanBeLocked(IopConcept entity)
    {
        var userHasRole = _userContextService.GetUserBusinessRole()
            is BusinessRole.LocalDataSteward or BusinessRole.InteroperabilityService;

        var userBelongsToAgency = _userContextService.UserBelongsToAgency(entity.Publisher.Identifier);

        if (!userBelongsToAgency || !userHasRole)
        {
            throw new ForbiddenException($"No authorization to lock the resource.");
        }

        if (entity.IsLocked)
        {
            throw new InvalidOperationException($"The resource is already locked.");
        }
    }

    private void EnsureConceptCanBeUnlocked(IopConcept entity)
    {
        var userHasRole = _userContextService.GetUserBusinessRole() is BusinessRole.InteroperabilityService;
        var userBelongsToAgency = _userContextService.UserBelongsToAgency(entity.Publisher.Identifier);

        if (!userBelongsToAgency || !userHasRole)
        {
            throw new ForbiddenException($"No authorization to unlock the resource.");
        }

        if (!entity.IsLocked)
        {
            throw new InvalidOperationException($"The resource is already unlocked.");
        }

        var isVocabulary = _vocabulariesService.GetVocabularyConfigs(default)
            .GetAwaiter()
            .GetResult()
            .Any(x => entity.Identifiers.Contains(x.ConceptIdentifier) && x.ConceptVersion == entity.Version);

        if (isVocabulary)
        {
            throw new MethodNotAllowedException("The resource is a used vocabulary and cannot be unlocked");
        }
    }

    protected override IQueryable<IopConcept> CreateGetAuthorizedEntitiesQuery(
        Expression<Func<IopConcept, bool>>? filter,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel)
    {
        filter ??= x => true;

        var userHasValidToken = _userContextService.IsUserTokenValid();

        var query = asNoTracking
            ? _dbContext.IopConcepts.AsNoTracking()
            : _dbContext.IopConcepts.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, userHasValidToken),
            _ => query.Include(c => c.Publisher)
        };

        AppendUserReadAuthorizationConditionToDatabaseQuery(ref query);
        return query.Where(filter);

        static IQueryable<IopConcept> buildEntityIncludeLevelAllQuery(
            IQueryable<IopConcept> query,
            bool userHasValidToken)
        {
            query = query
                .Include(c => c.ConformsTo)
                .Include(c => c.Replaces)
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

    private IQueryable<CodeListEntry> CreateGetConceptCodeListEntriesQueryWithFilter(
        Guid conceptId,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<CodeListEntry, bool>> filter)
    {
        var query = CreateGetCodeListEntriesQueryWithFilter(asNoTracking, entityIncludeLevel, filter);

        return query.Where(x => x.IopConceptId == conceptId);
    }

    private IQueryable<CodeListEntry> CreateGetCodeListEntriesQueryWithFilter(
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<CodeListEntry, bool>> filter)
    {
        var query = asNoTracking
           ? _dbContext.CodeListEntries.AsNoTracking()
           : _dbContext.CodeListEntries.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => query
                .Include(c => c.Annotations.OrderBy(a => a.Position))
                .Include(c => c.ParentCodeListEntry),
            _ => query
        };

        return query.Where(filter);
    }

    private async Task<CodeListEntry> GetEnsuredCodeListEntryEntity(
        Guid conceptId,
        Guid codeListEntryId,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        // Ensure user can get entity
        _ = await GetEnsuredEntity(conceptId, asNoTracking: true, cancellationToken: cancellationToken);

        var query = CreateGetConceptCodeListEntriesQueryWithFilter(
            conceptId,
            asNoTracking,
            entityIncludeLevel,
            x => true);

        var entity = await query
            .SingleOrDefaultAsync(c => c.Id == codeListEntryId, cancellationToken)
            ?? throw new NotFoundException($"No resource has been found.");

        return entity;
    }

    private async Task<PagedResult<CodeListEntry>> GetEnsuredSortedCodeListEntryEntities(
        IopConcept ensuredConceptEntity,
        Expression<Func<CodeListEntry, bool>>? filter = null,
        CodeListEntrySortProperty? sortProperty = null,
        SortOrder sortOrder = SortOrder.Ascending,
        int page = 1,
        int pageSize = int.MaxValue,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        if (ensuredConceptEntity.ConceptType is not ConceptType.CodeList)
        {
            return new()
            {
                Page = page,
                PageSize = pageSize,
                Results = [],
                TotalCount = 0,
            };
        }

        filter ??= x => true;
        sortProperty ??= ensuredConceptEntity.CodeListEntryDefaultSortProperty;

        var query = CreateGetConceptCodeListEntriesQueryWithFilter(
            ensuredConceptEntity.Id,
            asNoTracking,
            entityIncludeLevel,
            filter);

        query = appendCodeListEntriesSortingToQuery(
            query,
            ensuredConceptEntity.CodeListEntryValueType!.Value,
            sortProperty,
            sortOrder);

        var totalCount = await query.CountAsync(cancellationToken);

        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CodeListEntry>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = result,
            TotalCount = totalCount
        };

        static IQueryable<CodeListEntry> appendCodeListEntriesSortingToQuery(
            IQueryable<CodeListEntry> query,
            CodeListEntryValueType codeValueType,
            CodeListEntrySortProperty? sortProperty,
            SortOrder sortOrder)
        {
            sortProperty ??= default;

            query = sortProperty switch
            {
                CodeListEntrySortProperty.Code when codeValueType is CodeListEntryValueType.String => query.OrderBy(x => x.Code),
                CodeListEntrySortProperty.Code when codeValueType is CodeListEntryValueType.Numeric => query.OrderBy(x => Convert.ToDouble(x.Code)),
                CodeListEntrySortProperty.Position => query.OrderBy(x => x.Position),
                _ => throw new NotImplementedException($"The sort property '{sortProperty}' is not supported.")
            };

            return sortOrder is SortOrder.Ascending
                ? query
                : query.Reverse();
        }
    }

    private async Task<Guid> GetCodeListEntryIdFromCode(Guid conceptId, string code, CancellationToken cancellationToken)
    {
        var codeListEntry = await _dbContext.CodeListEntries
            .SingleOrDefaultAsync(c => c.IopConceptId == conceptId && c.Code == code, cancellationToken) ??
                throw new NotFoundException($"No codelist entry with the code '{code}' exists in the concept with the id '{conceptId}'.");

        return codeListEntry.Id;
    }
    /// <summary>
    /// Builds the forward <c>replaces</c> references from the stored URIs+names, additionally
    /// resolving the referenced concept's id when it exists on this platform and is readable by
    /// the caller (so the admin UI can link internally). The id is never stored.
    /// </summary>
    private async Task<IReadOnlyList<ResourceModel>> ResolveReplacesInput(
        IEnumerable<IdModel> ids,
        CancellationToken cancellationToken)
    {
        var resources = new List<ResourceModel>();

        foreach (var item in ids)
        {
            var concept = await _dbContext.IopConcepts
                .AsNoTracking()
                .Where(c => c.Id == item.Id)
                .Select(c => new { c.Identifiers, c.Version, c.Name })
                .FirstOrDefaultAsync(cancellationToken);

            if (concept is null || concept.Identifiers.Length == 0)
            {
                continue;
            }

            resources.Add(new ResourceModel
            {
                Uri = IriHelper.BuildConceptIri(_baseIriUrl, concept.Identifiers[0], concept.Version),
                Label = concept.Name.MapToMultiLanguageModel()
            });
        }

        return resources;
    }

    private async Task<IReadOnlyList<ConceptReferenceModel>> ResolveReplaces(
        IopConcept concept,
        CancellationToken cancellationToken)
    {
        var references = new List<ConceptReferenceModel>(concept.Replaces.Count);

        foreach (var resource in concept.Replaces)
        {
            Guid? conceptId = null;

            if (IriHelper.TryExtractConceptIdentifierAndVersion(resource.Href, out var identifier, out var version))
            {
                conceptId = await CreateGetAuthorizedEntitiesQuery(
                        c => c.Identifiers.Contains(identifier) && c.Version == version,
                        asNoTracking: true,
                        EntityIncludeLevel.Minimal)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (conceptId is null)
            {
                continue;
            }

            references.Add(new ConceptReferenceModel
            {
                Uri = resource.Href,
                Name = resource.Label?.MapToMultiLanguageModel(),
                ConceptId = conceptId
            });
        }

        return references;
    }

    /// <summary>
    /// Computes the inverse of <c>replaces</c>: the authorized concepts that declare this
    /// concept in their <c>replaces</c> list. Not stored — resolved on read (detail) only.
    /// </summary>
    private async Task<IReadOnlyList<ConceptReferenceModel>> GetIsReplacedBy(
        IopConcept concept,
        CancellationToken cancellationToken)
    {
        var identifier = concept.Identifiers.First(); 

        var thisIri = IriHelper.BuildConceptIri(_baseIriUrl, identifier, concept.Version);

        var replacingConcepts = await CreateGetAuthorizedEntitiesQuery(
                c => c.Replaces.Any(r => r.Href == thisIri),
                asNoTracking: true,
                EntityIncludeLevel.Minimal)
            .Select(c => new { c.Id, c.Identifiers, c.Version, c.Name })
            .ToListAsync(cancellationToken);

        return replacingConcepts
            .Where(c => c.Identifiers.Length > 0 && !string.IsNullOrWhiteSpace(c.Identifiers[0]))
            .Select(c => new ConceptReferenceModel
            {
                Uri = IriHelper.BuildConceptIri(_baseIriUrl, c.Identifiers[0], c.Version),
                Name = c.Name.MapToMultiLanguageModel(),
                ConceptId = c.Id
            })
            .ToList();
    }
}