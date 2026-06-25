using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class CreateCommandHandler : IRequestHandler<Admin.Commands.DatasetQualityInformation.CreateCommand, Models.DatasetQualityInformationData>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public CreateCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Models.DatasetQualityInformationData> Handle(Admin.Commands.DatasetQualityInformation.CreateCommand request, CancellationToken cancellationToken)
    {
        var data = _mapper.Map<DatasetQualityInformationDataModel>(request.DatasetQualityInformationData);

        await _apiClient.PostDatasetQualityInformationByBodyAsync(data, cancellationToken);

        return request.DatasetQualityInformationData;
    }
}