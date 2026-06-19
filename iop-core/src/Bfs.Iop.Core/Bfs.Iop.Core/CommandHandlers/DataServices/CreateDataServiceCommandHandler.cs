using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class CreateDataServiceCommandHandler : IRequestHandler<CreateDataServiceCommand, Guid>
{
    private readonly IDataServicesService _dataServicesService;

    public CreateDataServiceCommandHandler(IDataServicesService dataServicesService) => 
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(dataServicesService));

    public Task<Guid> Handle(CreateDataServiceCommand request, CancellationToken cancellationToken) => 
        _dataServicesService.AddDataService(request.InputModel, cancellationToken);
}
