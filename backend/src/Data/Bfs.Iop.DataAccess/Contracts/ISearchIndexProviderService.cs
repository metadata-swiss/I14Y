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

    IAsyncEnumerable<List<CodeListEntryModel>> GetCodeListEntriesInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);


    Task<DataServiceModel?> GetDataServiceById(Guid id, CancellationToken cancellationToken = default);

    Task<DcatDatasetModel?> GetDatasetById(Guid id, CancellationToken cancellationToken = default);

    Task<IopConceptModel?> GetIopConceptById(Guid id, CancellationToken cancellationToken = default);

    Task<MappingTableModel?> GetMappingTableById(Guid id, CancellationToken cancellationToken = default);

    Task<PublicServiceModel?> GetPublicServiceById(Guid id, CancellationToken cancellationToken = default);

    Task<List<CodeListEntryModel>> GetCodeListEntriesByConcept(
        Guid conceptId,
        CancellationToken cancellationToken = default);
}
