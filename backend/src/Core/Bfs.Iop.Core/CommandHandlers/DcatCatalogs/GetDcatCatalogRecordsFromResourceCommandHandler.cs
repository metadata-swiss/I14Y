using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogRecordsFromResourceCommandHandler : IRequestHandler<GetDcatCatalogRecordsFromResourceCommand, PagedResult<DcatCatalogRecordModel>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public GetDcatCatalogRecordsFromResourceCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<PagedResult<DcatCatalogRecordModel>> Handle(GetDcatCatalogRecordsFromResourceCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _dcatCatalogsService.GetDcatCatalogRecordsFromResource(request.ResourceId, page, pageSize, cancellationToken);
    }
}
