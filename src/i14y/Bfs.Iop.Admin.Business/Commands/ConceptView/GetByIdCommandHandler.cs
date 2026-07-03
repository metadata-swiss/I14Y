using Bfs.Iop.Admin.Commands.ConceptView;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptView;

internal class GetByIdCommandHandler : IRequestHandler<GetByIdCommand, Models.ConceptView>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetByIdCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Models.ConceptView> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var getConceptResponse = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(request.ConceptId, false, cancellationToken);

        return _mapper.Map<Models.ConceptView>(getConceptResponse.Result);
    }
}