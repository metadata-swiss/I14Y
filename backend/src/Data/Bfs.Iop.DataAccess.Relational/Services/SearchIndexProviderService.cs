using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Mappings;
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
            .Include(d => d.ResponsiblePerson);

    private IQueryable<Entities.CodeListEntry> CodeListEntries() =>
        _dbContext.CodeListEntries
            .AsNoTracking()
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
}
