using Bfs.Iop.Common.Messaging;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Messaging.SearchIndex;

internal sealed class SearchIndexNotifierService : ISearchIndexNotifierService
{
    private readonly IMessageQueue<SearchIndexMessage> _queue;

    public SearchIndexNotifierService(IMessageQueue<SearchIndexMessage> queue) =>
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));

    public Task NotifyResourceChangedAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default) =>
        EnqueueAsync(
            new SearchIndexMessage(SearchIndexTarget.CatalogResource, id, SearchIndexOperation.Upsert, type),
            cancellationToken);

    public Task NotifyResourceDeletedAsync(Guid id, CancellationToken cancellationToken = default) =>
        EnqueueAsync(
            new SearchIndexMessage(SearchIndexTarget.CatalogResource, id, SearchIndexOperation.Remove),
            cancellationToken);

    public Task NotifyCodeListChangedAsync(Guid conceptId, CancellationToken cancellationToken = default) =>
        EnqueueAsync(
            new SearchIndexMessage(SearchIndexTarget.CodeList, conceptId, SearchIndexOperation.Upsert),
            cancellationToken);

    public Task NotifyCodeListDeletedAsync(Guid conceptId, CancellationToken cancellationToken = default) =>
        EnqueueAsync(
            new SearchIndexMessage(SearchIndexTarget.CodeList, conceptId, SearchIndexOperation.Remove),
            cancellationToken);

    private Task EnqueueAsync(SearchIndexMessage message, CancellationToken cancellationToken) =>
        _queue.EnqueueAsync(message, cancellationToken).AsTask();
}
