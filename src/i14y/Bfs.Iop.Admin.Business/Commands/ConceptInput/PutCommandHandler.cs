using AutoMapper;
using Bfs.Iop.Admin.Commands;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput;

internal class PutCommandHandler : IRequestHandler<PutInputCommand<Models.ConceptInput>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public PutCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task Handle(PutInputCommand<Models.ConceptInput> request, CancellationToken cancellationToken)
    {
        var iopConceptInputModel = _mapper.Map<IopConceptInputModel>(request.Model);

        var existingConceptResponse = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(request.Model.Id, false, cancellationToken);

        var existingConcept = existingConceptResponse.Result;

        var existingConceptAsInputModel = _mapper.Map<IopConceptInputModel>(existingConcept);

        var updatedIopConceptInputModel = _mapper.Map(iopConceptInputModel, existingConceptAsInputModel);

        await _apiClient.PutConceptsByIdAndBodyAsync(request.Model.Id, updatedIopConceptInputModel, cancellationToken);
    }
}