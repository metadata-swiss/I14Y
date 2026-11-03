using Bfs.Iop.Admin.Commands;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using Cerise;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DataServiceInput;

internal class PutInputCommandHandler : AsyncRequestHandler<PutInputCommand<Models.DataServiceInput>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public PutInputCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    protected override Task Handle(PutInputCommand<Models.DataServiceInput> request, CancellationToken cancellationToken)
    {
        var inputModel = _mapper.Map<DataServiceInputModel>(request.Model);

        return _apiClient.PutDataServicesByIdAndBodyAsync(request.Model.Id, inputModel, cancellationToken);
    }
}