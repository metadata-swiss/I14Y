using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Relational.Tools;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class SearchIndexProviderService : ISearchIndexProviderService
{
    private readonly IopDbContext _dbContext;
    private readonly IVocabulariesService _vocabulariesService;

    public SearchIndexProviderService(
        IopDbContext dbContext,
        IVocabulariesService vocabulariesService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }


    private IQueryable<Entities.DataService> DataServices() =>
        _dbContext.DataServices
            .AsNoTracking()
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson)
            .Include(d => d.ContactPoint);

    private IQueryable<Entities.Dataset> Datasets() =>
        _dbContext.Datasets
            .AsNoTracking()
            .Include(d => d.Distributions)
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson)
            .Include(d => d.ContactPoint)
            .Include(d => d.QualifiedAttribution)
                .ThenInclude(qa => qa.Agent)
            .AsSplitQuery();

    private IQueryable<Entities.IopConcept> IopConcepts() =>
        _dbContext.IopConcepts
            .AsNoTracking()
            .Include(d => d.Keywords)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson);

    private IQueryable<Entities.MappingTable> MappingTables() =>
        _dbContext.MappingTables
            .AsNoTracking()
            .Include(d => d.Keywords)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson);

    private IQueryable<Entities.PublicService> PublicServices() =>
        _dbContext.PublicServices
            .AsNoTracking()
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson)
            .Include(d => d.Channels)
                .ThenInclude(c => c.OwnedBy)
                    .ThenInclude(o => o.OwnedBy)
            .AsSplitQuery();

    private IQueryable<Entities.CodeListEntry> CodeListEntries() =>
        _dbContext.CodeListEntries
            .AsNoTracking()
            .Include(c => c.ParentCodeListEntry)
            .Include(c => c.Annotations);

    public async IAsyncEnumerable<IEnumerable<DataServiceModel>> GetDataServicesInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<DataServiceModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in DataServices().ToAsyncEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(item.MapToDataServiceModel(_vocabulariesService));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<DataServiceModel>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async IAsyncEnumerable<IEnumerable<DcatDatasetModel>> GetDatasetsInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<DcatDatasetModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in Datasets().ToAsyncEnumerable())
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

    public async IAsyncEnumerable<IEnumerable<IopConceptModel>> GetIopConceptsInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<IopConceptModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in IopConcepts().ToAsyncEnumerable())
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

    public async IAsyncEnumerable<IEnumerable<MappingTableModel>> GetMappingTablesInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<MappingTableModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in MappingTables().ToAsyncEnumerable())
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

    public async IAsyncEnumerable<IEnumerable<PublicServiceModel>> GetPublicServicesInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<PublicServiceModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in PublicServices().ToAsyncEnumerable())
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

    public async IAsyncEnumerable<List<CodeListEntryModel>> GetCodeListEntriesInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<CodeListEntryModel>(batchSize);

        await foreach (var codeListEntryEntity in CodeListEntries().ToAsyncEnumerable())
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

    // The single-resource reads below exist so that one changed resource costs one indexed lookup
    // rather than a full table scan. Each returns null when the row is gone, which is the ordinary
    // case for a notification that arrives after a delete.

    public async Task<DataServiceModel?> GetDataServiceById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        var entity = await DataServices().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToDataServiceModel(_vocabulariesService);
    }

    public async Task<DcatDatasetModel?> GetDatasetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        var entity = await Datasets().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToDcatDatasetModel(_vocabulariesService);
    }

    public async Task<IopConceptModel?> GetIopConceptById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        var entity = await IopConcepts().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToIopConceptModel(_vocabulariesService);
    }

    public async Task<MappingTableModel?> GetMappingTableById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        var entity = await MappingTables().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToMappingTableModel(_vocabulariesService);
    }

    public async Task<PublicServiceModel?> GetPublicServiceById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        var entity = await PublicServices().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity?.MapToPublicServiceModel(_vocabulariesService);
    }

    // Whole concept, not one entry: an entry's ancestor codes are derived from its siblings, so
    // re-indexing one entry in isolation would leave the breadcrumbs of its descendants stale.
    public async Task<List<CodeListEntryModel>> GetCodeListEntriesByConcept(
        Guid conceptId,
        CancellationToken cancellationToken = default)
    {
        var entities = await CodeListEntries()
            .Where(x => x.IopConceptId == conceptId)
            .ToListAsync(cancellationToken);

        return [.. entities.Select(x => x.MapToCodeListEntryModel())];
    }

    public async Task<IReadOnlyList<ConceptIdentityModel>> GetConceptIdentities(
        CancellationToken cancellationToken = default)
    {
        var concepts = await _dbContext.IopConcepts
            .AsNoTracking()
            .Select(x => new { x.Id, x.Identifiers, x.Version })
            .ToListAsync(cancellationToken);

        return
        [
            .. concepts
                .Where(x => x.Identifiers.Length > 0
                    && !string.IsNullOrWhiteSpace(x.Identifiers[0])
                    && !string.IsNullOrWhiteSpace(x.Version))
                .Select(x => new ConceptIdentityModel(x.Id, x.Identifiers[0], x.Version)),
        ];
    }

    public async Task<IReadOnlySet<Guid>> GetPublicDatasetIds(CancellationToken cancellationToken = default)
    {
        var ids = await OnlyPublic(_dbContext.Datasets)
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicReuseCounts(
        CancellationToken cancellationToken = default)
    {
        var counts = new Dictionary<Guid, int>();

        await AddDataServicesServingDatasets(counts, cancellationToken);
        await AddDatasetsReferencingDataServices(counts, cancellationToken);
        await AddPublicServicesDescribedAtDatasets(counts, cancellationToken);
        await AddPublicServicesReferencingPublicServices(counts, cancellationToken);
        await AddMappingTablesReferencingConcepts(counts, cancellationToken);

        return counts.AsReadOnly();
    }

    private static IQueryable<T> OnlyPublic<T>(IQueryable<T> query)
        where T : Entities.PublishableEntityBase =>
        query.Where(x => x.PublicationLevel == PublicationLevel.Public);

    private static void Accumulate(Dictionary<Guid, int> counts, IEnumerable<(Guid Id, int Count)> rows)
    {
        foreach (var (id, count) in rows)
        {
            counts[id] = counts.GetValueOrDefault(id) + count;
        }
    }

    private async Task AddDataServicesServingDatasets(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from relation in _dbContext.DataServiceDatasetRelations
            join dataService in OnlyPublic(_dbContext.DataServices)
                on relation.DataServiceId equals dataService.Id
            group dataService.Id by relation.DatasetId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToListAsync(cancellationToken);

        Accumulate(counts, rows.Select(x => (x.Id, x.Count)));
    }

    private async Task AddDatasetsReferencingDataServices(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from relation in _dbContext.DataServiceDatasetRelations
            join dataset in OnlyPublic(_dbContext.Datasets)
                on relation.DatasetId equals dataset.Id
            group dataset.Id by relation.DataServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToListAsync(cancellationToken);

        Accumulate(counts, rows.Select(x => (x.Id, x.Count)));
    }

    private async Task AddPublicServicesDescribedAtDatasets(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from relation in _dbContext.PublicServiceDescribedAtDatasetRelations
            join publicService in OnlyPublic(_dbContext.PublicServices)
                on relation.PublicServiceId equals publicService.Id
            group publicService.Id by relation.IsDescribedAtId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToListAsync(cancellationToken);

        Accumulate(counts, rows.Select(x => (x.Id, x.Count)));
    }

    private async Task AddPublicServicesReferencingPublicServices(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var linkedTo = await (
            from relation in _dbContext.PublicServicesRelations
            join source in OnlyPublic(_dbContext.PublicServices)
                on relation.PublicServiceId equals source.Id
            group source.Id by relation.RelationId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToListAsync(cancellationToken);

        Accumulate(counts, linkedTo.Select(x => (x.Id, x.Count)));

        var required = await (
            from requires in _dbContext.Set<Entities.PublicServiceRequires>()
            join source in OnlyPublic(_dbContext.PublicServices)
                on requires.PublicServiceId equals source.Id
            group source.Id by requires.RequiresId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToListAsync(cancellationToken);

        Accumulate(counts, required.Select(x => (x.Id, x.Count)));
    }

    private async Task AddMappingTablesReferencingConcepts(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var concepts = await GetConceptIdentities(cancellationToken);

        if (concepts.Count == 0)
        {
            return;
        }

        var conceptIds = new Dictionary<(string Identifier, string Version), Guid>();

        foreach (var concept in concepts)
        {
            conceptIds.TryAdd((concept.Identifier, concept.Version), concept.Id);
        }

        var mappingTables = await OnlyPublic(_dbContext.MappingTables)
            .AsNoTracking()
            .Select(x => new { x.SourceUri, x.TargetUri })
            .ToListAsync(cancellationToken);

        var referenced = new HashSet<Guid>();

        foreach (var mappingTable in mappingTables)
        {
            referenced.Clear();

            // Once per concept even when both ends name it, matching the relations endpoint.
            foreach (var uri in new[] { mappingTable.SourceUri, mappingTable.TargetUri })
            {
                if (uri is not null
                    && IriHelper.TryExtractConceptIdentifierAndVersion(uri, out var identifier, out var version)
                    && conceptIds.TryGetValue((identifier, version), out var conceptId))
                {
                    referenced.Add(conceptId);
                }
            }

            Accumulate(counts, referenced.Select(x => (x, 1)));
        }
    }
}
