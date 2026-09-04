using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class UpdateDcatCatalogRecordCommandHandler : IRequestHandler<UpdateDcatCatalogRecordCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public UpdateDcatCatalogRecordCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task Handle(UpdateDcatCatalogRecordCommand request, CancellationToken cancellationToken)
    {
        return _dcatCatalogsService.UpdateDcatCatalogRecord(request.DcatCatalogId, request.DcatCatalogRecordId, request.UpdateModel, cancellationToken);
    }
}
