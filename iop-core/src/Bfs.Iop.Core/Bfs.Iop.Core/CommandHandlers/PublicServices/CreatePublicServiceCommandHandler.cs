using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class CreatePublicServiceCommandHandler : IRequestHandler<CreatePublicServiceCommand, Guid>
{
    private IPublicServicesService _publicServicesService;

    public CreatePublicServiceCommandHandler(IPublicServicesService publicServicesService) => 
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));

    public Task<Guid> Handle(CreatePublicServiceCommand request, CancellationToken cancellationToken) =>
        _publicServicesService.AddPublicService(request.InputModel, cancellationToken);
}
