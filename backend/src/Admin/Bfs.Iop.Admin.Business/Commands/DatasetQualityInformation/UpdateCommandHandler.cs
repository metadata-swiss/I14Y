using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using Cerise;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class UpdateCommandHandler : AsyncRequestHandler<UpdateCommand>
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

    protected override Task Handle(UpdateCommand request, CancellationToken cancellationToken)
    {
        var data = _mapper.Map<DatasetQualityInformationDataModel>(request.DatasetQualityInformationData);

        return _apiClient.PutDatasetQualityInformationByBodyAsync(data, cancellationToken);
    }
}