using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetPublicServiceCommandHandler : IRequestHandler<GetPublicServiceCommand, PublicServiceModel>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetPublicServiceCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));

    public Task<PublicServiceModel> Handle(GetPublicServiceCommand request, CancellationToken cancellationToken) =>
        _publicServicesService.GetPublicService(request.Id, cancellationToken);
}
