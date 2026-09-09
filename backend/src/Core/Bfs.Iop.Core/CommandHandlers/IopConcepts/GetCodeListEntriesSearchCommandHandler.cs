using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesSearchCommandHandler :
    IRequestHandler<GetCodeListEntriesSearchCommand, PagedResult<CodeListEntrySearchResultEntryModel>>
{
    private readonly ICodeListEntrySearchService _codeListEntryLuceneService;

    public GetCodeListEntriesSearchCommandHandler(ICodeListEntrySearchService codeListEntryLuceneService) 
        => _codeListEntryLuceneService = codeListEntryLuceneService ??
            throw new ArgumentNullException(nameof(codeListEntryLuceneService));

    public Task<PagedResult<CodeListEntrySearchResultEntryModel>> Handle(
        GetCodeListEntriesSearchCommand request,
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _codeListEntryLuceneService.Search(
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