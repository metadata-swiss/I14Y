using Bfs.Iop.Admin.Commands.DatasetInput;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetInput;

internal sealed class GetDcatDatasetInputModelCommandHandler : IRequestHandler<GetDcatDatasetInputModelCommand, DcatDatasetInputModel>
{
    private readonly IIopCoreApiClient _apiClient;

    public GetDcatDatasetInputModelCommandHandler(IIopCoreApiClient apiClient) => 
        _apiClient = apiClient;

    public async Task<DcatDatasetInputModel> Handle(GetDcatDatasetInputModelCommand request, CancellationToken cancellationToken)
    {
        var model = (await _apiClient.GetDatasetsByIdAsync(request.Id, cancellationToken)).Result;

        var json = JsonSerializer.Serialize(model);
        var input = JsonSerializer.Deserialize<DcatDatasetInputModel>(json);

        return input!;
    }
}