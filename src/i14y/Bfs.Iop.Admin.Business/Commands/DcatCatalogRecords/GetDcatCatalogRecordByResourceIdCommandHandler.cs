using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalogRecords;

internal class GetDcatCatalogRecordByResourceIdCommandHandler : IRequestHandler<GetDcatCatalogRecordByResourceIdCommand, IEnumerable<DcatCatalogRecordInput>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetDcatCatalogRecordByResourceIdCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<IEnumerable<DcatCatalogRecordInput>> Handle(GetDcatCatalogRecordByResourceIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDcatCatalogsRecordsFromResourceByResourceIdAndPageAndPageSizeAsync(
            request.Id,
            null, 
            null,
            cancellationToken);

        var results = _mapper.Map<IEnumerable<DcatCatalogRecordInput>>(response.Result);

        foreach ( var result in results )
        {
            var dcatCatalog = (await _apiClient.GetDcatCatalogsByIdAsync(result.CatalogId, cancellationToken)).Result;
            result.CatalogTitle = _mapper.Map<MultiLanguage>(dcatCatalog.Title);
        }

        return results;
    }
}