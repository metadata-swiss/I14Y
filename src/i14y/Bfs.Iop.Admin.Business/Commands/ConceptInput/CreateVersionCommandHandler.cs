using Bfs.Iop.Admin.Commands.ConceptInput;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput;

public class CreateVersionCommandHandler : IRequestHandler<CreateVersionCommand, Models.ConceptInput>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public CreateVersionCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Models.ConceptInput> Handle(CreateVersionCommand request, CancellationToken cancellationToken)
    {
        var existingIopConceptResponse = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(
            request.Model.PreviousVersionId,
            false,
            cancellationToken);

        var existingIopConcept = existingIopConceptResponse.Result;

        var inputModel = _mapper.Map<IopConceptInputModel>(existingIopConcept);

        var newVersionModel = inputModel with
        {
            Description = _mapper.Map<MultiLanguageModel>(request.Model.Description),
            Name = _mapper.Map<MultiLanguageModel>(request.Model.Name),
            ResponsibleDeputy = request.Model.ResponsibleDeputy is not null 
                ? new EmailInputModel() { Email = request.Model.ResponsibleDeputy.Identifier }
                : null,
            ResponsiblePerson = new EmailInputModel() { Email = request.Model.ResponsiblePerson.Identifier },
            ValidFrom = request.Model.ValidFrom,
            ValidTo = request.Model.ValidTo,
            Version = request.Model.Version,
        };

        var postConceptVersionsResponse = await _apiClient.PostConceptsVersionsByIdAndBodyAsync(
            request.Model.PreviousVersionId, 
            newVersionModel,
            cancellationToken);

        var id = postConceptVersionsResponse.Result;

        var getIopConceptResponse = await _apiClient.GetConceptsByIdAndIncludeCodeListEntriesAsync(id, false, cancellationToken);

        return _mapper.Map<Models.ConceptInput>(getIopConceptResponse.Result);
    }
}