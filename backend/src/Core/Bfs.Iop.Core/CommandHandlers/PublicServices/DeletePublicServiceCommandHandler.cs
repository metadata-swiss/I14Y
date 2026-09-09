using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class DeletePublicServiceCommandHandler : IRequestHandler<DeletePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public DeletePublicServiceCommandHandler(
        IPublicServicesService publicServicesService,
        ICatalogIndexService catalogIndexService)
    {
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(DeletePublicServiceCommand request, CancellationToken cancellationToken)
    {
        await _publicServicesService.DeletePublicService(request.Id, cancellationToken);
        _catalogIndexService.DeIndex(request.Id);
    }
}
