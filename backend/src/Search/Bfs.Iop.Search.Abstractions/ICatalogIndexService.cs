using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Writes and queries the catalog index of the IndexSearch service.
/// <para>
/// These contracts belong to the IndexSearch service and are deliberately independent of the
/// in-process Lucene implementation that still lives in Bfs.Iop.Core.Lucene. The two run in
/// different processes and are free to evolve separately; nothing here may depend on Lucene.
/// </para>
/// </summary>
public interface ICatalogIndexService
{
    /// <summary>
    /// Creates the index and its mapping if missing. When <paramref name="recreate"/> is true the
    /// existing index is dropped first, which discards every document.
    /// </summary>
    Task EnsureIndexAsync(bool recreate, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(DcatDatasetModel model, bool? hasStructure = null, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<DcatDatasetModel> models, IEnumerable<string> datasetsStructuresFileNames, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<PublicServiceModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<DataServiceModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<IopConceptModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<MappingTableModel> models, CancellationToken cancellationToken = default);

    Task DeIndexAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<PagedResult<CatalogSearchResultEntry>> SearchAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<CatalogSearchCountResultEntry>> SearchCountAsync(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        CancellationToken cancellationToken = default);
}
