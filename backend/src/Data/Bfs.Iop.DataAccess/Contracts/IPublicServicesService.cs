using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface IPublicServicesService : IPublishableEntityService
{
    Task<PublicServiceModel> GetPublicService(Guid id, CancellationToken cancellationToken = default);

    Task<PublicServiceModel> GetPublicServiceByIdentifier(string identifier, CancellationToken cancellationToken = default);

    Task<PagedResult<PublicServiceModel>> GetPublicServicesByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<ChannelModel> GetChannelByIdentifier(string identifier, CancellationToken cancellationToken = default);

    Task<Guid> AddPublicService(PublicServiceInputModel inputModel, CancellationToken cancellationToken = default);

    Task UpdatePublicService(
        Guid id,
        PublicServiceInputModel updateModel,
        CancellationToken cancelToken = default);

    Task<PagedResult<PublicServiceModel>> GetPublicServices(
        string? publicServiceIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<PublicServiceModel>> GetPublicServicesForIndexInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    Task DeletePublicService(Guid id, CancellationToken cancellationToken = default);
}
