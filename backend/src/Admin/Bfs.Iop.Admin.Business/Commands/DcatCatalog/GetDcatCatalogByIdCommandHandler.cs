using Bfs.Iop.Admin.Commands.DcatCatalog;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalog;

internal class GetDcatCatalogByIdCommandHandler : IRequestHandler<GetDcatCatalogByIdCommand, Models.DcatCatalog>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetDcatCatalogByIdCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Models.DcatCatalog> Handle(GetDcatCatalogByIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsByIdAsync(request.Id, cancellationToken);

        return _mapper.Map<Models.DcatCatalog>(response.Result);
    }
}
