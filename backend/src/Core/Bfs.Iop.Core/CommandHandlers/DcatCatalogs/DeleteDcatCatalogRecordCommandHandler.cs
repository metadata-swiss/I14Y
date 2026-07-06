using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class DeleteDcatCatalogRecordCommandHandler : IRequestHandler<DeleteDcatCatalogRecordCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public DeleteDcatCatalogRecordCommandHandler(IDcatCatalogsService dcatCatalogsService) =>
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task Handle(DeleteDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        return _dcatCatalogsService.DeleteDcatCatalogRecord(request.DcatCatalogId, request.DcatCatalogRecordId, cancellationToken);
    }
}
