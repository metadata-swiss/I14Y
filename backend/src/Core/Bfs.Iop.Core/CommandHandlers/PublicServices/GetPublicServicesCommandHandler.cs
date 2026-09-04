using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class GetPublicServicesCommandHandler : IRequestHandler<GetPublicServicesCommand, PagedResult<PublicServiceModel>>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetPublicServicesCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));

    public Task<PagedResult<PublicServiceModel>> Handle(GetPublicServicesCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _publicServicesService.GetPublicServices(
            request.PublicServiceIdentifier,
            request.PublisherIdentifier,
            request.PublicationLevel,
            request.RegistrationStatus,
            page,
            pageSize,
            cancellationToken);
    }
}
