using AutoMapper;
using Bfs.Iop.Admin.Commands.DcatCatalog;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalog;

internal class GetAllDcatCatalogsByUserCommandHandler : IRequestHandler<GetAllDcatCatalogsByUserCommand, IEnumerable<Models.DcatCatalog>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetAllDcatCatalogsByUserCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Models.DcatCatalog>> Handle(GetAllDcatCatalogsByUserCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsUserAgentsByPageAndPageSizeAsync(null, null, cancellationToken);
        return _mapper.Map<IEnumerable<Models.DcatCatalog>>(response.Result);
    }
}