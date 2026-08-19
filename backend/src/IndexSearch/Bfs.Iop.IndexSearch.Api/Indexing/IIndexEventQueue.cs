namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// In-memory buffer between the trigger endpoint and the worker that writes to Elasticsearch.
/// <para>
/// The queue is bounded and NOT durable: on restart, pending events are lost. That is a deliberate
/// trade — the periodic full reindex is what repairs any resulting drift, which is why it is a
/// required part of the deployment rather than an optimisation.
/// </para>
/// </summary>
public interface IIndexEventQueue
{
    /// <summary>Number of events waiting to be processed. Surfaced by GET /api/Index.</summary>
    int Count { get; }

    /// <summary>
    /// Enqueues an event. Returns false when the queue is full, so the caller can answer 429 and let
    /// the sender back off, rather than dropping work silently.
    /// </summary>
    bool TryEnqueue(IndexEvent indexEvent);

    /// <summary>Reads events until the queue is empty or <paramref name="maxItems"/> is reached.</summary>
    ValueTask<IReadOnlyCollection<IndexEvent>> DequeueBatchAsync(int maxItems, CancellationToken cancellationToken);
}
