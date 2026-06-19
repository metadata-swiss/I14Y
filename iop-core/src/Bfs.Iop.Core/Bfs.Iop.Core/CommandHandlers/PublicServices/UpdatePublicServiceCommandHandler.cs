using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class UpdatePublicServiceCommandHandler : IRequestHandler<UpdatePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;

    public UpdatePublicServiceCommandHandler(IPublicServicesService publicServicesService) => 
        _publicServicesService = publicServicesService
            ?? throw new ArgumentNullException(nameof(publicServicesService));

    public Task Handle(UpdatePublicServiceCommand request, CancellationToken cancellationToken) =>
        _publicServicesService.UpdatePublicService(request.Id, request.Model, cancellationToken);
}
