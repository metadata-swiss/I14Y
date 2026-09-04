using Bfs.Iop.Admin.Commands;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.PublicServiceInput;

internal class PutInputCommandHandler : IRequestHandler<PutInputCommand<Models.PublicServiceInput>>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public PutInputCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public Task Handle(PutInputCommand<Models.PublicServiceInput> request, CancellationToken cancellationToken)
    {
        var inputModel = _mapper.Map<PublicServiceInputModel>(request.Model);

        return _apiClient.PutPublicServicesByIdAndBodyAsync(request.Model.Id, inputModel, cancellationToken);
    }
}