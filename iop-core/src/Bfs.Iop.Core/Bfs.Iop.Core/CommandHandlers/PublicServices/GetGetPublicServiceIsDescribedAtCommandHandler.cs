using Bfs.Iop.Core.Abstractions.Commands.PublicServices;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.PublicServices;

internal sealed class GetGetPublicServiceIsDescribedAtCommandHandler : IRequestHandler<GetPublicServiceIsDescribedAtCommand, PagedResult<DcatDatasetModel>>
{
    private readonly IPublicServicesService _publicServicesService;
    private readonly IDatasetsService _datasetsService;

    public GetGetPublicServiceIsDescribedAtCommandHandler(
        IPublicServicesService publicServicesService, 
        IDatasetsService datasetsService)
    {
        _publicServicesService = publicServicesService;
        _datasetsService = datasetsService;
    }

    public async Task<PagedResult<DcatDatasetModel>> Handle(GetPublicServiceIsDescribedAtCommand request, CancellationToken cancellationToken)
    {
        var publicService = await _publicServicesService.GetPublicService(request.PublicServiceId, cancellationToken);

        return await _datasetsService.GetDatasetsByIds(publicService.IsDescribedAt.Select(x => x.Id), cancellationToken);
    }
}
