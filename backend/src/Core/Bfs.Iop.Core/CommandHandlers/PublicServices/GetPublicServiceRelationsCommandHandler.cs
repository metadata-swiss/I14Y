using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetPublicServiceRelationsCommandHandler : IRequestHandler<GetPublicServiceRelationsCommand, PagedResult<PublicServiceModel>>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetPublicServiceRelationsCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService;

    public async Task<PagedResult<PublicServiceModel>> Handle(GetPublicServiceRelationsCommand request, CancellationToken cancellationToken)
    {
        var publicService = await _publicServicesService.GetPublicService(request.PublicServiceId, cancellationToken);

        return await _publicServicesService.GetPublicServicesByIds(publicService.Relations.Select(x => x.Id), cancellationToken);
    }
}
