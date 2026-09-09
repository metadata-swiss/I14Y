using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class GetDatasetQualityQuestionsCommandHandler :
    IRequestHandler<GetDatasetQualityQuestionsCommand, PagedResult<DatasetQualityQuestionModel>>
{
    private readonly IDatasetsService _datasetsService;

    public GetDatasetQualityQuestionsCommandHandler(IDatasetsService datasetsService) =>
        _datasetsService = datasetsService;

    public Task<PagedResult<DatasetQualityQuestionModel>> Handle(
        GetDatasetQualityQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _datasetsService.GetQualityQuestions(page, pageSize, cancellationToken);
    }
}
