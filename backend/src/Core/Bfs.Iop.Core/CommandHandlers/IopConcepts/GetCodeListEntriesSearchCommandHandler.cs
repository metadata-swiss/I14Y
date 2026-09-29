using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Search;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesSearchCommandHandler :
    IRequestHandler<GetCodeListEntriesSearchCommand, PagedResult<CodeListEntrySearchResultEntryModel>>
{
    private readonly ICodeListEntryIndexSearch _search;

    public GetCodeListEntriesSearchCommandHandler(ICodeListEntryIndexSearch search)
        => _search = search ?? throw new ArgumentNullException(nameof(search));

    public Task<PagedResult<CodeListEntrySearchResultEntryModel>> Handle(
        GetCodeListEntriesSearchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, CodeListEntryIndexSearch.DefaultPageSize);

        return _search.SearchAsync(
            request.ConceptId,
            request.Language,
            request.Query,
            request.Filters,
            request.AddCodeListEntriesPaths,
            page,
            pageSize,
            cancellationToken);
    }
}
