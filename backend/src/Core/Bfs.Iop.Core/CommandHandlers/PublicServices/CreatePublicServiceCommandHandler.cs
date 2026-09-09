using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class CreatePublicServiceCommandHandler : IRequestHandler<CreatePublicServiceCommand, Guid>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public CreatePublicServiceCommandHandler(
        IPublicServicesService publicServicesService,
        ICatalogIndexService catalogIndexService)
    {
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task<Guid> Handle(CreatePublicServiceCommand request, CancellationToken cancellationToken)
    { 
        var id = await _publicServicesService.AddPublicService(request.InputModel, cancellationToken);

        var resource = await _publicServicesService.GetPublicService(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        return id;
    }
}
