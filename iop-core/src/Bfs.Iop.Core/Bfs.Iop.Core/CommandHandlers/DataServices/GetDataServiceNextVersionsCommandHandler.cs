using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class GetDataServiceNextVersionsCommandHandler : IRequestHandler<GetDataServiceNextVersionsCommand, PagedResult<DataServiceModel>>
{
    private readonly IDataServicesService _dataServicesService;

    public GetDataServiceNextVersionsCommandHandler(IDataServicesService datasetsService) =>
        _dataServicesService = datasetsService ??
            throw new ArgumentNullException(nameof(datasetsService));

    public Task<PagedResult<DataServiceModel>> Handle(
        GetDataServiceNextVersionsCommand request, 
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _dataServicesService.GetDataServiceNextVersions(
            request.DataServiceId,
            page,
            pageSize,
            cancellationToken);
    }
}
