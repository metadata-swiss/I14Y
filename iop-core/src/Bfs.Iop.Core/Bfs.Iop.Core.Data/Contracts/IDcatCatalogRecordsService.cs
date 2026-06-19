using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IDcatCatalogRecordsService : IAuthorizedEntityService
{
    Task<IEnumerable<DcatCatalogRecordModel>> GetDcatCatalogRecordsByCatalogId(
        Guid dcatCatalogId, 
        CancellationToken cancellationToken = default);
}
