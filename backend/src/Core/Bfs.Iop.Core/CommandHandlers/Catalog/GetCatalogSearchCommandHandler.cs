using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.IndexSearch.ApiClient;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

/// <summary>
/// Fulfils catalog search by calling the standalone IndexSearch service. Core holds no index of its
/// own, so this is the only route; there is no local engine to fall back to.
/// <para>
/// The client forwards the caller's bearer token, so results stay scoped to the signed-in user
/// exactly as they were when the index was in-process. Losing that token does not fail — it narrows
/// the results to public-only and still returns HTTP 200.
/// </para>
/// <para>
/// The publisher and vocabulary resolution this handler used to do is gone, not lost: the service
/// behind the port already returns finished <see cref="SearchResultModel"/>s. Re-mapping here would
/// be a second copy of that logic on the near side of the wire, free to drift. (It also resolved
/// publishers once per hit rather than once per distinct id — an N+1 that the implementation behind
/// the port does not reproduce.)
/// </para>
/// </summary>
internal sealed class GetCatalogSearchCommandHandler : IRequestHandler<GetCatalogSearchCommand, PagedResult<SearchResultModel>>
{
    private readonly IIndexSearchSearchClient _searchClient;

    public GetCatalogSearchCommandHandler(IIndexSearchSearchClient searchClient) =>
        _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));

    public Task<PagedResult<SearchResultModel>> Handle(GetCatalogSearchCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Unchanged from the original handler: no paging means "everything". Kept here rather than
        // relied upon downstream so the contract of this command stays self-contained.
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _searchClient.SearchAsync(
            request.Query,
            request.Language,
            request.Filter,
            page,
            pageSize,
            cancellationToken);
    }
}
