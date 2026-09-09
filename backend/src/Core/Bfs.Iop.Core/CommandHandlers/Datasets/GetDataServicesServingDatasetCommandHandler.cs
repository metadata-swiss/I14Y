using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDataServicesServingDatasetCommandHandler : 
    IRequestHandler<GetDataServicesServingDatasetCommand, PagedResult<DataServiceModel>>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IDataServicesService _dataServicesService;

    public GetDataServicesServingDatasetCommandHandler(
        IDatasetsService datasetsService,
        IDataServicesService dataServicesService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
    }

    public async Task<PagedResult<DataServiceModel>> Handle(
        GetDataServicesServingDatasetCommand request,
        CancellationToken cancellationToken)
    {
        // Ensure dataset exists and user can access it
        _ = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return await _dataServicesService.GetDataServicesServingDataset(
            request.DatasetId, 
            page,
            pageSize,
            cancellationToken);
    }
}
