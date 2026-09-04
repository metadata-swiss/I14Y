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

    public async IAsyncEnumerable<IEnumerable<DataServiceModel>> GetDataServicesInBatches(
        int batchSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DataServices.AsNoTracking();

        query = query
            .Include(d => d.Keyword)
            .Include(d => d.Publisher)
            .Include(d => d.ResponsibleDeputy)
            .Include(d => d.ResponsiblePerson)
            .Include(d => d.ContactPoint);

        var batch = new List<DataServiceModel>(batchSize);

        // Build the vocabulary cache, otherwise the asyncEnumerable call won't work
        await _vocabulariesService.BuildAllVocabulariesInCache(cancellationToken);

        await foreach (var item in query.ToAsyncEnumerable())
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

    public async IAsyncEnumerable<IEnumerable<IopConceptModel>> GetIopConceptsInBatches(
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

    public async IAsyncEnumerable<IEnumerable<MappingTableModel>> GetMappingTablesInBatches(
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

    public async IAsyncEnumerable<IEnumerable<PublicServiceModel>> GetPublicServicesInBatches(
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
}
