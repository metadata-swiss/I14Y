using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class GetDataServicesCommandHandler : IRequestHandler<GetDataServicesCommand, PagedResult<DataServiceModel>>
{
    private readonly IDataServicesService _dataServicesService;

    public GetDataServicesCommandHandler(IDataServicesService dataServicesService) =>
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));

    public Task<PagedResult<DataServiceModel>> Handle(GetDataServicesCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _dataServicesService.GetDataServices(
            request.AccessRights,
            request.DataServiceIdentifier,
            request.PublisherIdentifier,
            request.PublicationLevel,
            request.RegistrationStatus,
            page,
            pageSize,
            cancellationToken);
    }
}
