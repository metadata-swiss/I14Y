using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesForAutoCompleteCommandHandler : 
    IRequestHandler<GetCodeListEntriesForAutoCompleteCommand, PagedResult<CodeListEntryModel>>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntriesForAutoCompleteCommandHandler(IIopConceptsService conceptsService) => 
        _conceptsService = conceptsService ??
            throw new ArgumentNullException(nameof(conceptsService));

    public Task<PagedResult<CodeListEntryModel>> Handle(
        GetCodeListEntriesForAutoCompleteCommand request,
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _conceptsService.GetCodeListEntriesForAutoComplete(
            request.ConceptId,
            request.CodePrefix,
            request.SortProperty,
            request.SortOrder,
            page,
            pageSize,
            cancellationToken);
    }
}
