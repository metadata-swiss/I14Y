using Bfs.Iop.Core.Abstractions.Commands.DataServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DataServices;

internal sealed class GetDataServiceByIdentifierCommandHandler : IRequestHandler<GetDataServiceByIdentifierCommand, DataServiceModel>
{
    private readonly IDataServicesService _dataServicesService;

    public GetDataServiceByIdentifierCommandHandler(IDataServicesService datasetsService) => 
        _dataServicesService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

    public Task<DataServiceModel> Handle(GetDataServiceByIdentifierCommand request, CancellationToken cancellationToken)
        => _dataServicesService.GetDataServiceByIdentifier(request.Identifier, cancellationToken);
}
