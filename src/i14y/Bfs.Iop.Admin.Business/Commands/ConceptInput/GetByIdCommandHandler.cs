using AutoMapper;
using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput;

internal class GetByIdCommandHandler : IRequestHandler<GetInputCommand<Models.ConceptInput>, Models.ConceptInput>
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

    public async Task<Models.ConceptInput> Handle(GetInputCommand<Models.ConceptInput> request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
            request.Id,
            includeCodeListEntries: false,
            cancellationToken);

        return _mapper.Map<Models.ConceptInput>(response.Result);
    }
}