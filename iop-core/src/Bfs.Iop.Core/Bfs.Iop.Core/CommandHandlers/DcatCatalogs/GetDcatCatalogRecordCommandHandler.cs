using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogRecordCommandHandler : IRequestHandler<GetDcatCatalogRecordCommand, DcatCatalogRecordModel>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public GetDcatCatalogRecordCommandHandler(IDcatCatalogsService dcatCatalogsService) =>
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<DcatCatalogRecordModel> Handle(GetDcatCatalogRecordCommand request, CancellationToken cancellationToken) =>
        _dcatCatalogsService.GetDcatCatalogRecord(request.DcatCatalogId, request.DcatCatalogRecordId, cancellationToken);
}
