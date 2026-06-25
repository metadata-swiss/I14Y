using AutoMapper;
using Bfs.Iop.Admin.Commands.DataServiceInput;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DataServiceInput;

internal class GetDataServiceDatasetsCommandHandler : IRequestHandler<GetDataServiceDatasetsCommand, IEnumerable<Dataset>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetDataServiceDatasetsCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Dataset>> Handle(GetDataServiceDatasetsCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDataServicesByIdAsync(request.DataServiceId, cancellationToken);

        var datasets = new List<Dataset>();

        foreach (var datasetIdModel in response.Result.ServesDatasets)
        {
            var datasetResponse = await _apiClient.GetDatasetsByIdAsync(datasetIdModel.Id, cancellationToken);

            datasets.Add(_mapper.Map<Dataset>(datasetResponse.Result));
        }

        return datasets;
    }
}