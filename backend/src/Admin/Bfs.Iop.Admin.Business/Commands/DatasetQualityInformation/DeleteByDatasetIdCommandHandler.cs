using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.ApiClient;
using Cerise;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class DeleteByDatasetIdCommandHandler : AsyncRequestHandler<DeleteByDatasetIdCommand>
{
    private readonly IIopCoreApiClient _apiClient;

    public DeleteByDatasetIdCommandHandler(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    protected override Task Handle(DeleteByDatasetIdCommand request, CancellationToken cancellationToken) => 
        _apiClient.DeleteDatasetQualityInformationByDatasetIdAsync(request.DatasetId, cancellationToken);
}