using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.IndexSearch.ApiClient;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Catalog;

/// <summary>
/// Fulfils the catalog facet counts by calling the standalone IndexSearch service — the same client,
/// and therefore the same index, that answers the result list these counts label.
/// <para>
/// The vocabulary and agent mapping this handler used to perform now happens inside the search
/// service, in the same place as the mapping for the result list it labels. That is deliberate: a
/// count and its results have to agree, and the surest way to keep them agreeing is for one component
/// to produce both. See <see cref="GetCatalogSearchCommandHandler"/>.
/// </para>
/// </summary>
internal sealed class GetCatalogSearchCountCommandHandler : IRequestHandler<GetCatalogSearchCountCommand, SearchCountResultModel>
{
    private readonly IIndexSearchSearchClient _searchClient;

    public GetCatalogSearchCountCommandHandler(IIndexSearchSearchClient searchClient) =>
        _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));

    public Task<SearchCountResultModel> Handle(GetCatalogSearchCountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _searchClient.SearchCountAsync(
            request.QueryString,
            request.Language,
            request.Filter,
            cancellationToken);
    }
}
