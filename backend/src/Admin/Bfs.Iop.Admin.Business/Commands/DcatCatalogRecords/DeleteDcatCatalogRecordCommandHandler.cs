using Bfs.Iop.Admin.Commands.DcatCatalogRecords;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DcatCatalogRecords;

internal class DeleteDcatCatalogRecordCommandHandler : IRequestHandler<DeleteDcatCatalogRecordCommand>
{
    private readonly IIopCoreApiClient _apiClient;

    public DeleteDcatCatalogRecordCommandHandler(IIopCoreApiClient client) => _apiClient = client;

    public Task Handle(DeleteDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        return _apiClient.DeleteDcatCatalogsRecordsByIdAndRecordIdAsync(request.CatalogId, request.CatalogRecordId, cancellationToken);
    }
}