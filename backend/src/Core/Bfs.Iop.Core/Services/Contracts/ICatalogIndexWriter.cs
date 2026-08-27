using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Services.Contracts;

/// <summary>
/// The catalog-index operations IOP Core performs: publish a resource, or remove it.
/// <para>
/// Core owns no index. Every member here ends up as an HTTP call to the IndexSearch service, made
/// off the request path — see <c>Bfs.Iop.Core.IndexForwarding</c>.
/// </para>
/// <para>
/// Deliberately narrower than the engine's own <c>ICatalogIndexService</c>, which also carries reads
/// (<c>SearchAsync</c>, <c>SearchCountAsync</c>) and index lifecycle (<c>EnsureIndexAsync</c>) — Core
/// does neither. It answers reads by calling <c>IIndexSearchSearchClient</c> directly from its search
/// handlers, and dropping or recreating an index is authority that belongs to the service that owns
/// it. Widening this interface to match the engine's would put members here that Core's
/// implementation could only answer by throwing.
/// </para>
/// <para>
/// This is also why Core no longer references <c>Bfs.Iop.Search.Abstractions</c> at all: those are
/// the search service's internal engine contracts. Core's write side speaks this interface, which it
/// owns, and its read side speaks the search service's published client.
/// </para>
/// </summary>
internal interface ICatalogIndexWriter
{
    Task UpdateIndexAsync(DcatDatasetModel model, bool? hasStructure = null, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<PublicServiceModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<DataServiceModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<IopConceptModel> models, CancellationToken cancellationToken = default);

    Task UpdateIndexAsync(IEnumerable<MappingTableModel> models, CancellationToken cancellationToken = default);

    Task DeIndexAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
