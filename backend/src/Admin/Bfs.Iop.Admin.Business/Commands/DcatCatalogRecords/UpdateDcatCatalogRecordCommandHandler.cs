using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalogRecords;

internal class UpdateDcatCatalogRecordCommandHandler : IRequestHandler<UpdateDcatCatalogRecordCommand>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public UpdateDcatCatalogRecordCommandHandler(IMapper mapper, IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public Task Handle(UpdateDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        var model = _mapper.Map<DcatCatalogRecordInputModel>(request.Model);
        return _apiClient.PutDcatCatalogsRecordsByIdAndRecordIdAndBodyAsync(request.Model.CatalogId, request.Model.Id, model, cancellationToken);
    }
}