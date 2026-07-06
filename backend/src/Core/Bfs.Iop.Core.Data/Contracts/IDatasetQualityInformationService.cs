using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IDatasetQualityInformationService : IAuthorizedEntityService
{
    public Task<PagedResult<DatasetQualityQuestionModel>> GetQualityQuestions(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
