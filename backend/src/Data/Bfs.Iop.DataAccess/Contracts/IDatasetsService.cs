using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

public interface IDatasetsService : IPublishableEntityService
{
    Task<DcatDatasetModel> GetDataset(Guid id, CancellationToken cancellationToken = default);

    Task<DcatDatasetModel> GetDataset(string identifier, CancellationToken cancellationToken = default);

    Task<IEnumerable<DcatDatasetModel>> GetDatasets(
        IEnumerable<Guid> ids,
        EntityIncludeLevel includeLevel = EntityIncludeLevel.All,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DcatDatasetModel>> GetDatasetNextVersions(
        Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DcatDatasetModel>> GetDatasets(
        string? accessRights,
        string? datasetIdentifier,
        string? publisherIdentifier,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<DatasetQualityInformationDataModel> GetDatasetQualityInformation(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<DatasetQualityQuestionModel>> GetQualityQuestions(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Guid> AddDataset(DcatDatasetInputModel inputModel, CancellationToken cancellationToken = default);

    Task AddDatasetQualityInformation(Guid id, DatasetQualityInformationDataModel inputModel, CancellationToken cancellationToken = default);

    Task UpdateDataset(Guid id, DcatDatasetInputModel inputModel, CancellationToken cancellationToken = default);

    Task UpdateDatasetQualityInformation(Guid id, DatasetQualityInformationDataModel updateModel, CancellationToken cancellationToken = default);

    Task DeleteDataset(Guid id, CancellationToken cancellationToken = default);

    Task DeleteDatasetQualityInformation(Guid id, CancellationToken cancellationToken = default);
}