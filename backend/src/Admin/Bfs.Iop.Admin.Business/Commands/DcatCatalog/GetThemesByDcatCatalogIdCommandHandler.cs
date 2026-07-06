using Bfs.Iop.Admin.Commands.DcatCatalog;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalog;

internal class GetThemesByDcatCatalogIdCommandHandler : IRequestHandler<GetThemesByDcatCatalogIdCommand, IEnumerable<Models.DcatVocabularyEntry>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetThemesByDcatCatalogIdCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<Models.DcatVocabularyEntry>> Handle(GetThemesByDcatCatalogIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsThemesByIdAndPageAndPageSizeAsync(request.Id, null, null, cancellationToken);

        return _mapper.Map<IEnumerable<Models.DcatVocabularyEntry>>(response.Result);
    }
}