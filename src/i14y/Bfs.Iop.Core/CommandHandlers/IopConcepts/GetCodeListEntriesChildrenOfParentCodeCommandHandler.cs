using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesChildrenOfParentCodeCommandHandler : IRequestHandler<GetCodeListEntriesChildrenOfParentCodeCommand, PagedResult<CodeListEntryModel>>
{
    private readonly IIopConceptsService _conceptsService;

    public GetCodeListEntriesChildrenOfParentCodeCommandHandler(IIopConceptsService conceptsService) =>
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public Task<PagedResult<CodeListEntryModel>> Handle(GetCodeListEntriesChildrenOfParentCodeCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _conceptsService.GetCodeListEntriesChildrenOfParentCode(
            request.ConceptId,
            request.ParentCode,
            request.SortProperty,
            request.SortOrder,
            page,
            pageSize,
            cancellationToken);
    }
}
