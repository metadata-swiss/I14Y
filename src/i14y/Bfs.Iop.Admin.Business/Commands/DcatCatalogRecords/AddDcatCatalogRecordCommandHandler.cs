using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalogRecords;

internal class AddDcatCatalogRecordCommandHandler : IRequestHandler<AddDcatCatalogRecordCommand, DcatCatalogRecordInput>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public AddDcatCatalogRecordCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<DcatCatalogRecordInput> Handle(AddDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        var dcatCatalogId = request.Model.CatalogId;
        var catalogResponse = await _apiClient.GetDcatCatalogsByIdAsync(dcatCatalogId, cancellationToken);
        var dcatCatalog = catalogResponse.Result;

        var inputModel = _mapper.Map<DcatCatalogRecordInputModel>(request.Model);
        var response = await _apiClient.PostDcatCatalogsRecordsByIdAndBodyAsync(dcatCatalogId, inputModel, cancellationToken);

        request.Model.Id = response.Result;
        request.Model.CatalogTitle = _mapper.Map<MultiLanguage>(dcatCatalog.Title);

        return request.Model;
    }
}