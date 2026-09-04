using Bfs.Iop.Admin.Commands;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.PublicServiceInput;

internal class CreateCommandHandler : IRequestHandler<PostInputCommand<Models.PublicServiceInput>, Models.PublicServiceInput>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public CreateCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Models.PublicServiceInput> Handle(PostInputCommand<Models.PublicServiceInput> request, CancellationToken cancellationToken)
    {
        var inputModel = _mapper.Map<PublicServiceInputModel>(request.Model);
        var response = await _apiClient.PostPublicServicesByBodyAsync(inputModel, cancellationToken);

        request.Model.Id = response.Result;

        return request.Model;
    }
}