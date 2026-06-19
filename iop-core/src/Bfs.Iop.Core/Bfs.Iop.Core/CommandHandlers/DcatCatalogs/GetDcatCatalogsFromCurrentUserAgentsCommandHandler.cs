using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Infrastructure.Security.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogsFromCurrentUserAgentsCommandHandler :
    IRequestHandler<GetDcatCatalogsFromCurrentUserAgentsCommand, PagedResult<DcatCatalogModel>>
{
    private readonly IUserContextService _userContextService;
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public GetDcatCatalogsFromCurrentUserAgentsCommandHandler(
        IUserContextService userContextService, 
        IDcatCatalogsService dcatCatalogsService)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
    }

    public async Task<PagedResult<DcatCatalogModel>> Handle(GetDcatCatalogsFromCurrentUserAgentsCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        if (_userContextService.GetUserBusinessRole() is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return await _dcatCatalogsService.GetDcatCatalogs(page, pageSize, cancellationToken);
        }

        var userAgencies = _userContextService.GetUserAgencies();

        return await _dcatCatalogsService.GetDcatCatalogsFromPublishers(userAgencies, page, pageSize, cancellationToken);
    }
}
