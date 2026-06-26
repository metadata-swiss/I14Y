using AutoMapper;
using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class UpdateCommandHandler : IRequestHandler<UpdateCommand>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public UpdateCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public Task Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        var data = _mapper.Map<DatasetQualityInformationDataModel>(request.DatasetQualityInformationData);

        return _apiClient.PutDatasetQualityInformationByBodyAsync(data, cancellationToken);
    }
}