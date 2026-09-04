using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDatasetNextVersionsCommandHandler : IRequestHandler<GetDatasetNextVersionsCommand, PagedResult<DcatDatasetModel>>
{
    private readonly IDatasetsService _datasetsService;

    public GetDatasetNextVersionsCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService ?? 
            throw new ArgumentNullException(nameof(datasetsService));

    public Task<PagedResult<DcatDatasetModel>> Handle(GetDatasetNextVersionsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _datasetsService.GetDatasetNextVersions(request.DatasetId, page, pageSize, cancellationToken);
    }
}
