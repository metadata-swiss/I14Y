using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class UpdatePublicServiceCommandHandler : IRequestHandler<UpdatePublicServiceCommand>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdatePublicServiceCommandHandler(
        IPublicServicesService publicServicesService,
        ICatalogIndexService catalogIndexService)
    {
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdatePublicServiceCommand request, CancellationToken cancellationToken)
    {
        await _publicServicesService.UpdatePublicService(request.Id, request.Model, cancellationToken);

        var resource = await _publicServicesService.GetPublicService(request.Id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
