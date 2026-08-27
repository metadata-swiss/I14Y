using System.Runtime.CompilerServices;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Data.Indexing;

/// <inheritdoc cref="IIndexDataReader"/>
/// <remarks>
/// The <c>Include</c> sets below are carried over verbatim from the index-feed queries that used to
/// live on the business services. They are deliberately NOT rewritten as a <c>Select</c> projection:
/// the unit tests run on EF's in-memory provider, which executes LINQ-to-Objects and therefore
/// translates anything, so a projection Npgsql could not translate would pass every test green and
/// throw only against a real database — on the query that populates the entire index.
/// <para>
/// Each <c>Include</c> here corresponds to a field the document factory reads. Dropping one does not
/// fail: it produces documents with that field silently missing, which surfaces as results that do
/// not match rather than as an error.
/// </para>
/// </remarks>
internal sealed class IndexDataReader : IIndexDataReader
{
    private readonly IopDbContext _dbContext;

    public IndexDataReader(IopDbContext dbContext) => _dbContext = dbContext;

    public IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetDatasetsInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.Datasets.AsNoTracking()
                .Include(d => d.Distributions)
                .Include(d => d.Keyword)
                .Include(d => d.Publisher)
                .Include(d => d.ResponsibleDeputy)
                .Include(d => d.ResponsiblePerson)
                .Include(d => d.ContactPoint),
            IndexEntryFactory.FromDataset,
            batchSize,
            cancellationToken);

    public IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetDataServicesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.DataServices.AsNoTracking()
                .Include(d => d.Keyword)
                .Include(d => d.Publisher)
                .Include(d => d.ResponsibleDeputy)
                .Include(d => d.ResponsiblePerson)
                .Include(d => d.ContactPoint),
            IndexEntryFactory.FromDataService,
            batchSize,
            cancellationToken);

    public IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetPublicServicesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.PublicServices.AsNoTracking()
                .Include(d => d.Keyword)
                .Include(d => d.Publisher)
                .Include(d => d.ResponsibleDeputy)
                .Include(d => d.ResponsiblePerson)
                // Not in the original query: the document factory indexes channel e-mail addresses,
                // which the model mapping supplied. Without this the public-service e-mail search
                // returns nothing, with no error.
                .Include(d => d.Channels),
            IndexEntryFactory.FromPublicService,
            batchSize,
            cancellationToken);

    public IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetConceptsInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.IopConcepts.AsNoTracking()
                .Include(d => d.Keywords)
                .Include(d => d.Publisher)
                .Include(d => d.ResponsibleDeputy)
                .Include(d => d.ResponsiblePerson),
            IndexEntryFactory.FromConcept,
            batchSize,
            cancellationToken);

    public IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetMappingTablesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.MappingTables.AsNoTracking()
                .Include(d => d.Keywords)
                .Include(d => d.Publisher)
                .Include(d => d.ResponsibleDeputy)
                .Include(d => d.ResponsiblePerson),
            IndexEntryFactory.FromMappingTable,
            batchSize,
            cancellationToken);

    public IAsyncEnumerable<IReadOnlyList<CodeListIndexEntry>> GetCodeListEntriesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default) =>
        InBatches(
            _dbContext.CodeListEntries.AsNoTracking()
                .Include(c => c.Annotations)
                // Load-bearing: the index stores the parent's CODE and the row holds only its id.
                // Without this every entry indexes as a root and the hierarchy vanishes from search.
                .Include(c => c.ParentCodeListEntry),
            IndexEntryFactory.FromCodeListEntry,
            batchSize,
            cancellationToken);

    private static async IAsyncEnumerable<IReadOnlyList<TEntry>> InBatches<TEntity, TEntry>(
        IQueryable<TEntity> query,
        Func<TEntity, TEntry> project,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TEntity : class
    {
        var size = Math.Max(1, batchSize);
        var batch = new List<TEntry>(size);

        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            batch.Add(project(item));

            if (batch.Count >= size)
            {
                yield return batch;
                batch = new List<TEntry>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}
