using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetPublicServiceRequiresCommandHandler : IRequestHandler<GetPublicServiceRequiresCommand, PagedResult<PublicServiceModel>>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetPublicServiceRequiresCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService;

    public async Task<PagedResult<PublicServiceModel>> Handle(GetPublicServiceRequiresCommand request, CancellationToken cancellationToken)
    {
        var publicService = await _publicServicesService.GetPublicService(request.PublicServiceId, cancellationToken);

        return await _publicServicesService.GetPublicServicesByIds(publicService.Requires.Select(x => x.Id), cancellationToken);
    }
}
