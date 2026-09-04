using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface IDataServicesService : IPublishableEntityService
{
    Task<DataServiceModel> GetDataService(Guid id, CancellationToken cancellationToken = default);

    Task<DataServiceModel> GetDataServiceByIdentifier(string identifier, CancellationToken cancellationToken = default);

    Task<PagedResult<DataServiceModel>> GetDataServices(
        string? accessRights,
        string? dataServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<DataServiceModel>> GetDataServicesForIndexInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DataServiceModel>> GetDataServicesServingDataset(
        Guid datasetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DataServiceModel>> GetDataServicesFromDistributionAccessServices(
        Guid distributionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DataServiceModel>> GetDataServiceNextVersions(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Guid> AddDataService(DataServiceInputModel inputModel, CancellationToken cancellationToken = default);

    Task UpdateDataService(Guid id, DataServiceInputModel updateModel, CancellationToken cancellationToken = default);

    Task DeleteDataService(Guid id, CancellationToken cancellationToken = default);
}
