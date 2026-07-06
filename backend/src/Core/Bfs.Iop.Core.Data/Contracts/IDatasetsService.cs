using Bfs.Iop.Core.Abstractions.Models;
using System.Runtime.CompilerServices;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IDatasetsService : IPublishableEntityService
{
    Task<DcatDatasetModel> GetDataset(Guid id, CancellationToken cancellationToken = default);

    Task<DcatDatasetModel> GetDatasetByIdentifier(string identifier, CancellationToken cancellationToken = default);

    Task<PagedResult<DcatDatasetModel>> GetDatasetsByIds(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

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

    IAsyncEnumerable<IEnumerable<DcatDatasetModel>> GetDatasetsForIndexInBatches(
        int batchSize = 100,
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