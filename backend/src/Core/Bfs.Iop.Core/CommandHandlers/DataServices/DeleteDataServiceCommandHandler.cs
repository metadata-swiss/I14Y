using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class DeleteDataServiceCommandHandler : IRequestHandler<DeleteDataServiceCommand>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public DeleteDataServiceCommandHandler(
        IDataServicesService dataServicesService,
        ICatalogIndexService catalogIndexService)
    {
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(DeleteDataServiceCommand request, CancellationToken cancellationToken)
    {
        await _dataServicesService.DeleteDataService(request.Id, cancellationToken);

        _catalogIndexService.DeIndex(request.Id);
    }
}
