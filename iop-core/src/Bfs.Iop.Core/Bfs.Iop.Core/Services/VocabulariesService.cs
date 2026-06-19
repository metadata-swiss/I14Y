using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Validation.Extensions;
using Bfs.Iop.Core.Vocabularies;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Bfs.Iop.Core.Services;

internal sealed class VocabulariesService : AuthorizedEntityServiceBase<VocabularyConfig>, IVocabulariesService
{
    private readonly IopDbContext _dbContext;
    private readonly ConcurrentDictionary<string, VocabularyModel> _vocabularyCache = new();
    private readonly SemaphoreSlim _lock = new(initialCount: 1, maxCount: 1);

    public VocabulariesService(
        IopDbContext dbContext,
        IEntityAuthorizationService entityAuthorizationService,
        IUserContextService userContextService) : base(entityAuthorizationService, userContextService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    protected override IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDeleteEntity =>
        [
          BusinessRole.InteroperabilityService
        ];

    public async Task<VocabularyModel> GetVocabulary(
        string vocabularyIdentifier,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vocabularyIdentifier, nameof(vocabularyIdentifier));

        try
        {
            await _lock.WaitAsync(cancellationToken);

            if (_vocabularyCache.TryGetValue(vocabularyIdentifier, out var cachedVocabulary))
            {
                return cachedVocabulary;
            }

            var vocabularyModel = await ReadVocabulary(vocabularyIdentifier, cancellationToken);
            _vocabularyCache[vocabularyIdentifier] = vocabularyModel;

            return vocabularyModel;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T?> TryGetVocabulary<T>(CancellationToken cancellationToken = default) where T : IdentifiedVocabularyBase, new()
    {
        try
        {
            T vocabulary = new();
            var result = await GetVocabulary(vocabulary.Identifier, cancellationToken);

            return vocabulary with { Entries = result.Entries };
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<VocabularyModel?> TryGetVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vocabularyIdentifier, nameof(vocabularyIdentifier));

        try
        {
            return await GetVocabulary(vocabularyIdentifier, cancellationToken);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task BuildAllVocabulariesInCache(CancellationToken cancellationToken = default)
    {
        _vocabularyCache.Clear();

        var configs = await GetVocabularyConfigs(cancellationToken);

        foreach (var item in configs)
        {
            await GetVocabulary(item.VocabularyIdentifier, cancellationToken);
        }
    }

    public async Task<IEnumerable<VocabularyConfigModel>> GetVocabularyConfigs(CancellationToken cancellationToken)
    {
        var configs = await GetEnsuredEntitiesByFilter(filter: null, asNoTracking: true, cancellationToken);

        return configs.Select(x => x.MapToVocabularyConfigModel());
    }

    public async Task<VocabularyConfigModel> GetVocabularyConfig(Guid id, CancellationToken cancellationToken)
    {
        var config = await GetEnsuredEntity(id, asNoTracking: true, cancellationToken: cancellationToken);

        return config.MapToVocabularyConfigModel();
    }

    public async Task<Guid> AddVocabularyConfig(VocabularyConfigInputModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        EnsureUserCanCreateEntities();

        EnsureVocabularyConfigInputModelIsValid(model);

        var entity = model.MapToVocabularyConfig();

        await _dbContext.VocabularyConfigs.AddAsync(entity, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"VocabularyConfig could not be created. '{ex.Message}'");
        }

        return entity.Id;
    }

    public async Task UpdateVocabularyConfig(Guid id, VocabularyConfigInputModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        var entity = await GetEnsuredEntity(id, asNoTracking: false, cancellationToken: cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureVocabularyConfigInputModelIsValid(model);

        model.MapToVocabularyConfig(entity);

        _dbContext.VocabularyConfigs.Update(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"VocabularyConfig could not be updated: {ex.Message}");
        }
    }

    public async Task DeleteVocabularyConfig(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _dbContext.VocabularyConfigs.Remove(entity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"VocabularyConfig could not be deleted: {ex.Message}", ex);
        }
    }

    protected override async Task<VocabularyConfig> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking
            ? _dbContext.VocabularyConfigs.AsNoTracking()
            : _dbContext.VocabularyConfigs.AsQueryable();

        var entity = await query.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException($"No resource has been found.");

        return entity;
    }

    private async Task<IList<VocabularyConfig>> GetEnsuredEntitiesByFilter(
        Expression<Func<VocabularyConfig, bool>>? filter = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? _dbContext.VocabularyConfigs.AsNoTracking()
            : _dbContext.VocabularyConfigs.AsQueryable();

        query = query.Where(filter);

        return await query.ToListAsync(cancellationToken);
    }

    private async Task<VocabularyConfig> GetEnsuredVocabularyConfig(
        string vocabularyIdentifier,
        CancellationToken cancellationToken)
    {
        var configs = await GetEnsuredEntitiesByFilter(
            x => x.VocabularyIdentifier == vocabularyIdentifier,
            asNoTracking: true, cancellationToken);

        return configs.SingleOrDefault() ??
            throw new NotFoundException($"Vocabulary configuration with identifier '{vocabularyIdentifier}' was not found.");
    }

    private async Task<VocabularyModel> ReadVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken)
    {
        var config = await GetEnsuredVocabularyConfig(vocabularyIdentifier, cancellationToken);

        var codeListEntries = await GetCodeListEntries(config.ConceptIdentifier, config.ConceptVersion, cancellationToken);

        var entries = codeListEntries.Select(codeListEntry => codeListEntry.MapToVocabularyEntryModel());

        return new VocabularyModel()
        {
            Identifier = vocabularyIdentifier,
            Entries = entries.ToList().AsReadOnly()
        };
    }

    private async Task<IEnumerable<CodeListEntry>> GetCodeListEntries(
        string conceptIdentifier,
        string conceptVersion,
        CancellationToken cancellationToken)
    {
        var concept = await _dbContext.IopConcepts
            .Where(c => c.Identifiers.Contains(conceptIdentifier) && c.Version == conceptVersion)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException("Resource not found.");

        return await _dbContext.CodeListEntries
            .Where(x => x.IopConceptId == concept.Id)
            .Include(x => x.Annotations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private void EnsureVocabularyConfigInputModelIsValid(VocabularyConfigInputModel model, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model.VocabularyIdentifier, nameof(model.VocabularyIdentifier));
        ArgumentException.ThrowIfNullOrWhiteSpace(model.ConceptIdentifier, nameof(model.ConceptIdentifier));

        if (!model.ConceptVersion.IsValidVersion())
        {
            throw new ArgumentException("The concept version must be in the format 'xx.xx.xx', where xx is a number (e.g. '1.0.0').", nameof(model));
        }

        if (_dbContext.VocabularyConfigs.Any(x =>
            x.VocabularyIdentifier == model.VocabularyIdentifier &&
            id != null &&
            x.Id != id))
        {
            throw new ConflictException($"The identifier '{model.VocabularyIdentifier}' is already in use.");
        }

        var concept = _dbContext.IopConcepts.SingleOrDefault(x => x.Identifiers.Contains(model.ConceptIdentifier) && x.Version == model.ConceptVersion)
            ?? throw new NotFoundException($"The concept with identifier '{model.ConceptIdentifier}' and version '{model.ConceptVersion}' does not exist.");

        var anyCodeListEntry = _dbContext.CodeListEntries.Any(x => x.IopConceptId == concept.Id);

        if (concept.PublicationLevel is not PublicationLevel.Public)
        {
            throw new MethodNotAllowedException("The concept must be published.");
        }

        if (concept.ConceptType is not ConceptType.CodeList || !anyCodeListEntry)
        {
            throw new MethodNotAllowedException("The concept must be of the type 'CodeList' and contain at least one entry.");
        }

        if (!concept.IsLocked)
        {
            throw new MethodNotAllowedException("The concept must be locked.");
        }
    }
}