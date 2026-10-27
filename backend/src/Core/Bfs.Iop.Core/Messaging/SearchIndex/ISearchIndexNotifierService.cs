using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Messaging.SearchIndex;

/// <summary>
///     Tells the search service that a resource has changed, so that searches stop answering from the
///     state of the last nightly rebuild.
/// </summary>
internal interface ISearchIndexNotifierService
{
    Task NotifyResourceChangedAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default);

    Task NotifyResourceDeletedAsync(Guid id, CancellationToken cancellationToken = default);

    Task NotifyCodeListChangedAsync(Guid conceptId, CancellationToken cancellationToken = default);

    Task NotifyCodeListDeletedAsync(Guid conceptId, CancellationToken cancellationToken = default);
}
