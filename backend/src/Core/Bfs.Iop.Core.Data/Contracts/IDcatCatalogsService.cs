using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IDcatCatalogsService : IAuthorizedEntityService
{
    Task<DcatCatalogModel> GetDcatCatalog(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<DcatCatalogModel>> GetDcatCatalogs(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<DcatCatalogModel>> GetDcatCatalogsFromPublishers(
        IEnumerable<string> publisherIdentifiers,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Guid> AddDcatCatalog(DcatCatalogInputModel inputModel, CancellationToken cancellationToken = default);

    Task UpdateDcatCatalog(Guid id, DcatCatalogInputModel updateModel, CancellationToken cancellationToken = default);

    Task DeleteDcatCatalog(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<DcatCatalogRecordModel>> GetDcatCatalogRecords(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DcatCatalogRecordModel>> GetDcatCatalogRecordsFromResource(Guid resourceId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<DcatCatalogRecordModel> GetDcatCatalogRecord(Guid id, Guid dcatCatalogRecordId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Guid>> AddDcatCatalogRecords(Guid id, IEnumerable<DcatCatalogRecordInputModel> inputModels, CancellationToken cancellationToken = default);

    Task UpdateDcatCatalogRecord(Guid id, Guid dcatCatalogRecordId, DcatCatalogRecordInputModel updateModel, CancellationToken cancellationToken = default);

    Task DeleteDcatCatalogRecord(Guid id, Guid dcatCatalogRecordId, CancellationToken cancellationToken = default);
}
