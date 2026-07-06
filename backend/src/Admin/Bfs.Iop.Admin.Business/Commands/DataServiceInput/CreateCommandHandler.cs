using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DataServiceInput;

internal sealed class CreateCommandHandler : IRequestHandler<PostInputCommand<Models.DataServiceInput>, Models.DataServiceInput>
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

    public async Task<Models.DataServiceInput> Handle(PostInputCommand<Models.DataServiceInput> request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<DataServiceInputModel>(request.Model);

        var response = await _apiClient.PostDataServicesByBodyAsync(model, cancellationToken);

        request.Model.Id = response.Result;

        return request.Model;
    }
}