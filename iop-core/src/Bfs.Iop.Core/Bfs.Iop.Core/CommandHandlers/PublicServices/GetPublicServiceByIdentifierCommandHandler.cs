using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetPublicServiceByIdentifierCommandHandler : IRequestHandler<GetPublicServiceByIdentifierCommand, PublicServiceModel>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetPublicServiceByIdentifierCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));

    public Task<PublicServiceModel> Handle(GetPublicServiceByIdentifierCommand request, CancellationToken cancellationToken)
        => _publicServicesService.GetPublicServiceByIdentifier(request.Identifier, cancellationToken);
}
