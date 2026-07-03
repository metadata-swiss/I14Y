using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MapsterMapper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput.CodeListEntries.Annotations;

internal sealed class AddAnnotationCommandHandler : IRequestHandler<AddAnnotationCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public AddAnnotationCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Guid> Handle(AddAnnotationCommand request, CancellationToken cancellationToken)
    {
        var input = _mapper.Map<AnnotationInputModel>(request.Model);

        var codeListEntry = (await _apiClient.GetConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(
            request.ConceptId,
            request.CodeListEntryId,
            cancellationToken)).Result;

        var codeListInput = _mapper.Map<CodeListEntryInputModel>(codeListEntry);
        codeListInput.Annotations = codeListInput.Annotations.Append(input);

        _ = await _apiClient.PutConceptsCodelistEntriesByIdAndCodeListEntryIdAndBodyAsync(
            request.ConceptId,
            request.CodeListEntryId,
            codeListInput,
            cancellationToken);

        codeListEntry = (await _apiClient.GetConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(
            request.ConceptId,
            request.CodeListEntryId,
            cancellationToken)).Result;

        return codeListEntry.Annotations.Last().Id;
    }
}
