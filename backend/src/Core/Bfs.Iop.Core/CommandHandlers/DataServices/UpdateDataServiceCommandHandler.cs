using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class UpdateDataServiceCommandHandler : IRequestHandler<UpdateDataServiceCommand>
{
    private readonly IDataServicesService _dataServicesService;

    public UpdateDataServiceCommandHandler(IDataServicesService dataServicesService) => 
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));

    public Task Handle(UpdateDataServiceCommand request, CancellationToken cancellationToken)
    {
        return _dataServicesService.UpdateDataService(request.Id, request.InputModel, cancellationToken);
    }
}
