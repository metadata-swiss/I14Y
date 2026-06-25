using AutoMapper;
using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput;

internal class CreateCommandHandler : IRequestHandler<PostInputCommand<Models.ConceptInput>, Models.ConceptInput>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public CreateCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Models.ConceptInput> Handle(PostInputCommand<Models.ConceptInput> request, CancellationToken cancellationToken)
    {
        var conceptApiInputModel = _mapper.Map<IopConceptInputModel>(request.Model);

        var postConceptResponse = await _apiClient.PostConceptsByBodyAsync(conceptApiInputModel, cancellationToken);

        var getConceptResponse = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
            postConceptResponse.Result,
            false, 
            cancellationToken);

        return _mapper.Map<Models.ConceptInput>(getConceptResponse.Result);
    }        
}