using AutoMapper;
using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class GetByDatasetIdCommandHandler : IRequestHandler<GetByDatasetIdCommand, Models.DatasetQualityInformationData>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetByDatasetIdCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Models.DatasetQualityInformationData> Handle(GetByDatasetIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetQualityInformationByDatasetIdAsync(request.DatasetId, cancellationToken);

        return _mapper.Map<Models.DatasetQualityInformationData>(response.Result);
    }
}