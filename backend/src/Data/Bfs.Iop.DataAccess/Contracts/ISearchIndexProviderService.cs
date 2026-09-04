using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface ISearchIndexProviderService
{
    IAsyncEnumerable<IEnumerable<DataServiceModel>> GetDataServicesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<DcatDatasetModel>> GetDatasetsInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<IopConceptModel>> GetIopConceptsInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<MappingTableModel>> GetMappingTablesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<PublicServiceModel>> GetPublicServicesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);
}
