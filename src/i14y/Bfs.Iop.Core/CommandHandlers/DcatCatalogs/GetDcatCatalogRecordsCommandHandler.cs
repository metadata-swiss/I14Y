using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogRecordsCommandHandler : IRequestHandler<GetDcatCatalogRecordsCommand, PagedResult<DcatCatalogRecordModel>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public GetDcatCatalogRecordsCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<PagedResult<DcatCatalogRecordModel>> Handle(GetDcatCatalogRecordsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
           ? (request.Page.Value, request.PageSize.Value)
           : (1, int.MaxValue);

        return _dcatCatalogsService.GetDcatCatalogRecords(request.DcatCatalogId, page, pageSize, cancellationToken);
    }
}
