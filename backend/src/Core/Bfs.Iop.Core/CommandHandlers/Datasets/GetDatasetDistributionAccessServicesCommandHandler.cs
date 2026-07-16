using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDatasetDistributionAccessServicesCommandHandler : 
    IRequestHandler<GetDatasetDistributionAccessServicesCommand, PagedResult<DataServiceModel>>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IDataServicesService _dataServicesService;

    public GetDatasetDistributionAccessServicesCommandHandler(
        IDatasetsService datasetsService,
        IDataServicesService dataServicesService)
    {
        _dataServicesService = dataServicesService;
        _datasetsService = datasetsService;
    }

    public async Task<PagedResult<DataServiceModel>> Handle(GetDatasetDistributionAccessServicesCommand request, CancellationToken cancellationToken)
    {
        var dataset = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        if (dataset.Distributions.SingleOrDefault(x => x.Id == request.DistributionId) is null)
        {
            throw new NotFoundException("The distribution does not exist.");
        }

        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return await _dataServicesService.GetDataServicesFromDistributionAccessServices(
            request.DistributionId,
            page,
            pageSize,
            cancellationToken);
    }
}
