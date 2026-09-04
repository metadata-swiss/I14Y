using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record GetDatasetQualityQuestionsCommand(
    int? Page,
    int? PageSize) : IRequest<PagedResult<DatasetQualityQuestionModel>>
{ }
