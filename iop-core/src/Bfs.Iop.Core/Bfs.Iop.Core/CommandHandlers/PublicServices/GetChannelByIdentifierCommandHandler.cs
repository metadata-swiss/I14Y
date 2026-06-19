using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetChannelByIdentifierCommandHandler : IRequestHandler<GetChannelByIdentifierCommand, ChannelModel>
{
    private readonly IPublicServicesService _publicServicesService;

    public GetChannelByIdentifierCommandHandler(IPublicServicesService publicServicesService) => 
        _publicServicesService = publicServicesService;

    public Task<ChannelModel> Handle(GetChannelByIdentifierCommand request, CancellationToken cancellationToken) => 
        _publicServicesService.GetChannelByIdentifier(request.Identifier, cancellationToken);
}
