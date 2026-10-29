using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.Core.ApiClient;
using Cerise;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalogRecords;

internal class DeleteDcatCatalogRecordCommandHandler : AsyncRequestHandler<DeleteDcatCatalogRecordCommand>
{
    private readonly IIopCoreApiClient _apiClient;

    public DeleteDcatCatalogRecordCommandHandler(IIopCoreApiClient client) => _apiClient = client;

    protected override Task Handle(DeleteDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        return _apiClient.DeleteDcatCatalogsRecordsByIdAndRecordIdAsync(request.CatalogId, request.CatalogRecordId, cancellationToken);
    }
}