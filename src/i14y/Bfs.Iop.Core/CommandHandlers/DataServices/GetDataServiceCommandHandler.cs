using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class GetDataServiceCommandHandler : IRequestHandler<GetDataServiceCommand, DataServiceModel>
{
    private readonly IDataServicesService _dataServicesService;

    public GetDataServiceCommandHandler(IDataServicesService dataServicesService) => 
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));

    public Task<DataServiceModel> Handle(GetDataServiceCommand request, CancellationToken cancellationToken) => 
        _dataServicesService.GetDataService(request.DataServiceId, cancellationToken);
}
