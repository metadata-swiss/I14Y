using AutoMapper;
using Bfs.Iop.Admin.Commands.PublicServiceView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.PublicServiceView;

internal class GetByIdCommandHandler : IRequestHandler<GetByIdCommand, Models.PublicServiceView>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public GetByIdCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<Models.PublicServiceView> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetPublicServicesByIdAsync(request.Id, cancellationToken);
        var publicService = response.Result;

        var result = _mapper.Map<Models.PublicServiceView>(publicService);

        var isDescribedAtTask = _apiClient.GetPublicServicesIsDescribedAtByIdAsync(request.Id, cancellationToken);
        var relationsTask = _apiClient.GetPublicServicesRelationsByIdAsync(request.Id, cancellationToken);
        var requiresTask = _apiClient.GetPublicServicesRequiresByIdAsync(request.Id, cancellationToken);

        await Task.WhenAll(isDescribedAtTask, relationsTask, requiresTask);

        result.IsDescribedAt = _mapper.Map<IEnumerable<IdLabel>>(isDescribedAtTask.Result.Result);
        result.Relation = _mapper.Map<IEnumerable<IdLabel>>(relationsTask.Result.Result);
        result.Requires = _mapper.Map<IEnumerable<IdLabel>>(requiresTask.Result.Result);

        return result;
    }
}