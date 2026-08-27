using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.IndexSearch.ApiClient;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntriesSearchCommandHandler :
    IRequestHandler<GetCodeListEntriesSearchCommand, PagedResult<CodeListEntrySearchResultEntryModel>>
{
    private readonly IIndexSearchSearchClient _searchClient;

    public GetCodeListEntriesSearchCommandHandler(IIndexSearchSearchClient searchClient) 
        => _searchClient = searchClient ??
            throw new ArgumentNullException(nameof(searchClient));

    public Task<PagedResult<CodeListEntrySearchResultEntryModel>> Handle(
        GetCodeListEntriesSearchCommand request,
        CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _searchClient.SearchCodeListEntriesAsync(
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