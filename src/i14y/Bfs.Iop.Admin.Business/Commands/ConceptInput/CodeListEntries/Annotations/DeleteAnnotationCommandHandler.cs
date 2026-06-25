using AutoMapper;
using Bfs.Iop.Admin.Commands.ConceptInput.CodeListEntries.Annotations;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.ConceptInput.CodeListEntries.Annotations;

internal sealed class DeleteAnnotationCommandHandler : IRequestHandler<DeleteAnnotationCommand>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _apiClient;

    public DeleteAnnotationCommandHandler(
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task Handle(DeleteAnnotationCommand request, CancellationToken cancellationToken)
    {
        var codeListEntry = (await _apiClient.GetConceptsCodelistEntriesByIdAndCodeListEntryIdAsync(
            request.ConceptId,
            request.CodeListEntryId,
            cancellationToken)).Result;

        var indexOfAnnotationToDelete = codeListEntry
            .Annotations
            .Select(a => a.Id)
            .ToList()
            .IndexOf(request.AnnotationId);

        if (indexOfAnnotationToDelete < 0)
        {
            throw new InvalidOperationException("The annotation does not belong to the codelist entry.");
        }

        var codeListInput = _mapper.Map<CodeListEntryInputModel>(codeListEntry);
        var annotations = codeListInput.Annotations.ToList();
        annotations.RemoveAt(indexOfAnnotationToDelete);
        codeListInput.Annotations = annotations;

        await _apiClient.PutConceptsCodelistEntriesByIdAndCodeListEntryIdAndBodyAsync(
            request.ConceptId,
            request.CodeListEntryId,
            codeListInput,
            cancellationToken);
    }
}
