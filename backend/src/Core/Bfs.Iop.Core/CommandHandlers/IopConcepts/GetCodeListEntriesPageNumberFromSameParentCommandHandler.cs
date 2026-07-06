using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesPageNumberFromSameParentCommandHandler 
    : IRequestHandler<GetCodeListEntriesPageNumberFromSameParentCommand, int>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntriesPageNumberFromSameParentCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<int> Handle(
        GetCodeListEntriesPageNumberFromSameParentCommand request,
        CancellationToken cancellationToken) => 
        _conceptsService.GetCodeListEntriesPageNumberFromSameParent(
            request.ConceptId,
            request.Code,
            request.SortProperty,
            request.SortOrder,
            request.PageSize ?? int.MaxValue,
            cancellationToken);
}