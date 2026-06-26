using AutoMapper;
using Bfs.Iop.Admin.Commands.DistributionSummary;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DistributionSummary;

internal class GetAllByDatasetIdCommandHandler : IRequestHandler<GetAllByDatasetIdCommand, IEnumerable<Models.DistributionSummary>>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public GetAllByDatasetIdCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Models.DistributionSummary>> Handle(GetAllByDatasetIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsByIdAsync(request.DatasetId, cancellationToken);

        return _mapper.Map<IEnumerable<Models.DistributionSummary>>(response.Result.Distributions);
    }
}