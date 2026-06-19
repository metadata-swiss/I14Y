
using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetConceptStructureReferencesCommandHandler : IRequestHandler<GetConceptStructureReferencesCommand, PagedResult<IopConceptStructureReferenceModel>>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly IIopConceptsService _conceptsService;

    public GetConceptStructureReferencesCommandHandler(
        IDatasetModelProcessService datasetModelProcessService,
        IIopConceptsService conceptsService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));

        _conceptsService = conceptsService
            ?? throw new ArgumentNullException(nameof(conceptsService));
    }

    public async Task<PagedResult<IopConceptStructureReferenceModel>> Handle(GetConceptStructureReferencesCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        var concept = await _conceptsService.GetIopConcept(request.ConceptId, false, cancellationToken);

        return await _datasetModelProcessService.GetConceptStructureReferences(
            concept.Identifiers.First(), 
            concept.Version, 
            page,
            pageSize,
            cancellationToken);
    }  
}
