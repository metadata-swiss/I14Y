using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Lucene.Index;

public interface ICatalogIndexService
{
    void UpdateIndex(DcatDatasetModel model, bool? hasStructure = null);

    void UpdateIndex(IEnumerable<DcatDatasetModel> models, IEnumerable<string> datasetsStructuresFileNames);

    void UpdateIndex(params PublicServiceModel[] models);

    void UpdateIndex(params DataServiceModel[] models);

    void UpdateIndex(params IopConceptModel[] models);

    void UpdateIndex(params MappingTableModel[] models);

    void DeIndex(params Guid[] ids);

    PagedResult<CatalogSearchResultEntry> Search(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize);

    IEnumerable<CatalogSearchCountResultEntry> SearchCount(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter);
}
