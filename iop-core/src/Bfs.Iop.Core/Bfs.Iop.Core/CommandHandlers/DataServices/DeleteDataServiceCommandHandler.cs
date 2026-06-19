using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class DeleteDataServiceCommandHandler : IRequestHandler<DeleteDataServiceCommand>
{
    private readonly IDataServicesService _dataServicesService;

    public DeleteDataServiceCommandHandler(IDataServicesService dataServicesService) => 
        _dataServicesService = dataServicesService;

    public Task Handle(DeleteDataServiceCommand request, CancellationToken cancellationToken)
    {
        return _dataServicesService.DeleteDataService(request.Id, cancellationToken);
    }
}
