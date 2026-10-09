namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

/// <summary>
///     Reads back what the catalogue index already holds, for the fields a single-document write cannot
///     work out on its own.
/// </summary>
public interface ICatalogIndexReader
{
    /// <summary>
    ///     The structure flag currently indexed for a dataset, or null when the index holds no value -
    ///     because the document is not there, or was written before the flag existed.
    /// </summary>
    Task<bool?> ReadStructureFlagAsync(Guid datasetId, CancellationToken cancellationToken = default);
}
