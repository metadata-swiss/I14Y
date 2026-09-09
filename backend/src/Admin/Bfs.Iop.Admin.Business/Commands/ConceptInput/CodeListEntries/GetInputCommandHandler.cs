using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput.CodeListEntries;

internal sealed class GetInputCommandHandler : IRequestHandler<GetInputCommand, CodelistEntryInput>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public GetInputCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<CodelistEntryInput> Handle(GetInputCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(
            request.ConceptId,
            request.CodeListEntryId,
            cancellationToken);

        var model = _mapper.Map<CodelistEntryInput>(response.Result);
        model.ConceptId = request.ConceptId;

        return model;
    }
}
