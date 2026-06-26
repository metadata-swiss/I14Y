using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class DeleteByDatasetIdCommandHandler : IRequestHandler<DeleteByDatasetIdCommand>
{
    private readonly IIopCoreApiClient _apiClient;

    public DeleteByDatasetIdCommandHandler(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    public Task Handle(DeleteByDatasetIdCommand request, CancellationToken cancellationToken)
    {
        return _apiClient.DeleteDatasetQualityInformationByDatasetIdAsync(request.DatasetId, cancellationToken);
    }
}