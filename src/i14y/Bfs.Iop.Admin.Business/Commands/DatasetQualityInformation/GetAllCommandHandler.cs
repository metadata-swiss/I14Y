using AutoMapper;
using Bfs.Iop.Admin.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetQualityInformation;

internal class GetAllCommandHandler : IRequestHandler<GetAllCommand, IEnumerable<Models.DatasetQualityQuestion>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetAllCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Models.DatasetQualityQuestion>> Handle(GetAllCommand request, CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetDatasetQualityInformationDefinitionByPageAndPageSizeAsync(null, null, cancellationToken);

        return result.Result.Select(d => _mapper.Map<Models.DatasetQualityQuestion>(d));
    }
}