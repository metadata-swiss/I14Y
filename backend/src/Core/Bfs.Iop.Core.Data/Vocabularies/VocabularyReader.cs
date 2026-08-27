using System.Collections.Concurrent;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Vocabularies;

/// <inheritdoc cref="IVocabularyReader"/>
/// <remarks>
/// Scoped, and the cache with it: a vocabulary is a code list read out of the same database, so a
/// per-request cache keeps one search from re-reading the same five vocabularies once per hit while
/// still picking up an edit on the next request.
/// </remarks>
internal sealed class VocabularyReader : IVocabularyReader
{
    private readonly IopDbContext _dbContext;
    private readonly ConcurrentDictionary<string, VocabularyModel> _cache = new();
    private readonly SemaphoreSlim _lock = new(initialCount: 1, maxCount: 1);

    public VocabularyReader(IopDbContext dbContext) => _dbContext = dbContext;

    public async Task<T?> TryGetVocabulary<T>(CancellationToken cancellationToken = default)
        where T : IdentifiedVocabularyBase, new()
    {
        T vocabulary = new();
        var result = await TryGetVocabulary(vocabulary.Identifier, cancellationToken);

        return result is null ? null : vocabulary with { Entries = result.Entries };
    }

    public async Task<VocabularyModel?> TryGetVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vocabularyIdentifier, nameof(vocabularyIdentifier));

        if (_cache.TryGetValue(vocabularyIdentifier, out var cached))
        {
            return cached;
        }

        try
        {
            await _lock.WaitAsync(cancellationToken);

            if (_cache.TryGetValue(vocabularyIdentifier, out cached))
            {
                return cached;
            }

            var vocabulary = await ReadVocabulary(vocabularyIdentifier, cancellationToken);
            if (vocabulary is not null)
            {
                _cache[vocabularyIdentifier] = vocabulary;
            }

            return vocabulary;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task BuildAllVocabulariesInCache(CancellationToken cancellationToken = default)
    {
        var identifiers = await _dbContext.VocabularyConfigs
            .AsNoTracking()
            .Select(x => x.VocabularyIdentifier)
            .ToListAsync(cancellationToken);

        foreach (var identifier in identifiers)
        {
            await TryGetVocabulary(identifier, cancellationToken);
        }
    }

    private async Task<VocabularyModel?> ReadVocabulary(string vocabularyIdentifier, CancellationToken cancellationToken)
    {
        var config = await _dbContext.VocabularyConfigs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.VocabularyIdentifier == vocabularyIdentifier, cancellationToken);

        if (config is null)
        {
            return null;
        }

        var concept = await _dbContext.IopConcepts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                c => c.Identifiers.Contains(config.ConceptIdentifier) && c.Version == config.ConceptVersion,
                cancellationToken);

        if (concept is null)
        {
            return null;
        }

        var codeListEntries = await _dbContext.CodeListEntries
            .Where(x => x.IopConceptId == concept.Id)
            .Include(x => x.Annotations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new VocabularyModel
        {
            Identifier = vocabularyIdentifier,
            Entries = codeListEntries.Select(ToEntry).ToList().AsReadOnly(),
        };
    }

    private static VocabularyEntryModel ToEntry(CodeListEntry entity) => new()
    {
        Code = entity.Code,
        Name = new MultiLanguageModel
        {
            De = entity.Name.De,
            En = entity.Name.En,
            Fr = entity.Name.Fr,
            It = entity.Name.It,
            Rm = entity.Name.Rm,
        },
        Uri = entity.Annotations.FirstOrDefault(x => x.Type == ExternalResourceAnnotationType)?.Uri,
    };

    /// <summary>The annotation that carries a vocabulary entry's canonical URI.</summary>
    private const string ExternalResourceAnnotationType = "EXT_RESOURCE";
}
