using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class DeletePublicServiceCommandHandler : IRequestHandler<DeletePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;

    public DeletePublicServiceCommandHandler(IPublicServicesService publicServicesService) =>
        _publicServicesService = publicServicesService;

    public Task Handle(DeletePublicServiceCommand request, CancellationToken cancellationToken)
    {
        return _publicServicesService.DeletePublicService(request.Id, cancellationToken);
    }
}
