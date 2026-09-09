using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class CreateDataServiceCommandHandler : IRequestHandler<CreateDataServiceCommand, Guid>
{
    private readonly IDataServicesService _dataServicesService;
    private readonly ICatalogIndexService _catalogIndexService;

    public CreateDataServiceCommandHandler(
        IDataServicesService dataServicesService,
        ICatalogIndexService catalogIndexService)
    {
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));     
    }

    public async Task<Guid> Handle(CreateDataServiceCommand request, CancellationToken cancellationToken)
    {
        var id = await _dataServicesService.AddDataService(request.InputModel, cancellationToken);

        var resource = await _dataServicesService.GetDataService(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);

        return id;
    }
}
