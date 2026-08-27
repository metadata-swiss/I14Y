using Bfs.Iop.Core.Abstractions.Models.Indexing;

namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Streams the search corpus straight out of the database, already flattened into the shape the
/// index stores.
/// <para>
/// This exists so the IndexSearch service can rebuild the index without referencing
/// <c>Bfs.Iop.Core</c>. It reads the same tables the business services read, but produces
/// <see cref="CatalogIndexEntry"/> directly instead of going through domain models — which used to
/// mean resolving every vocabulary code into a labelled object that the document factory then reduced
/// back to the same code.
/// </para>
/// <para>
/// <b>Deliberately unauthorized, and it must stay that way.</b> The index has to contain non-public
/// resources: filtering happens at query time from the caller's claims, so an entitled user can only
/// find their own unpublished work if it was indexed in the first place. Adding a user filter here
/// would empty the index of everything private, silently.
/// </para>
/// </summary>
public interface IIndexDataReader
{
    /// <summary>
    /// Datasets. <see cref="CatalogIndexEntry.HasStructure"/> is left null — it comes from the object
    /// store, which this reader knows nothing about, and the caller fills it in.
    /// </summary>
    IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetDatasetsInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetDataServicesInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetPublicServicesInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetConceptsInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<CatalogIndexEntry>> GetMappingTablesInBatches(int batchSize = 100, CancellationToken cancellationToken = default);

    IAsyncEnumerable<IReadOnlyList<CodeListIndexEntry>> GetCodeListEntriesInBatches(int batchSize = 100, CancellationToken cancellationToken = default);
}
