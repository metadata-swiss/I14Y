using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.IndexForwarding;

/// <summary>
/// Accepts index writes from IOP Core's request threads and hands them to a background sender.
/// </summary>
public interface IIndexSearchDispatcher
{
    /// <summary>
    /// Queues one resource for forwarding. Never blocks and never throws: the index is a side
    /// effect of a save, so a failure here must not fail the user's save.
    /// </summary>
    void Enqueue(IndexForwardItem item);

    /// <summary>Reads queued items, waiting for at least one.</summary>
    ValueTask<IReadOnlyCollection<IndexForwardItem>> DequeueBatchAsync(int maxItems, CancellationToken cancellationToken);
}

/// <summary>
/// A bounded in-memory channel.
/// <para>
/// The queue is not durable, and on overflow it drops the oldest pending work rather than blocking
/// the caller. Both are deliberate: IOP Core's index contracts are synchronous <c>void</c> methods
/// called inside a save, so there is nowhere to surface backpressure or an error to. What makes
/// that acceptable is the IndexSearch service's periodic full reindex, which re-reads everything
/// from Postgres and repairs whatever was dropped.
/// </para>
/// </summary>
internal sealed class IndexSearchDispatcher : IIndexSearchDispatcher
{
    /// <summary>
    /// Maximum pending forwards held in memory. A constant rather than a setting: the configuration
    /// section that used to bind it appeared in no appsettings file, so this value was the only one
    /// ever in effect and the knob was tunable in appearance only.
    /// </summary>
    private const int QueueCapacity = 10_000;

    private readonly Channel<IndexForwardItem> _channel;
    private readonly ILogger<IndexSearchDispatcher> _logger;
    private long _droppedCount;

    public IndexSearchDispatcher(ILogger<IndexSearchDispatcher> logger)
    {
        _logger = logger;

        _channel = Channel.CreateBounded<IndexForwardItem>(
            new BoundedChannelOptions(QueueCapacity)
            {
                // Drop the oldest rather than the newest: recent edits are the ones a user is most
                // likely to be looking for in search right now.
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false,
            },
            OnItemDropped);
    }

    public void Enqueue(IndexForwardItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // With DropOldest a write only fails once the channel is completed, i.e. at shutdown.
        _channel.Writer.TryWrite(item);
    }

    public async ValueTask<IReadOnlyCollection<IndexForwardItem>> DequeueBatchAsync(int maxItems, CancellationToken cancellationToken)
    {
        var first = await _channel.Reader.ReadAsync(cancellationToken);

        var batch = new List<IndexForwardItem>(Math.Min(maxItems, 64)) { first };

        while (batch.Count < maxItems && _channel.Reader.TryRead(out var next))
        {
            batch.Add(next);
        }

        return batch;
    }

    private void OnItemDropped(IndexForwardItem item)
    {
        var total = Interlocked.Increment(ref _droppedCount);

        // Log the first drop and then every thousandth: a full queue means the sender cannot keep
        // up, and one line per dropped item would bury that signal in its own noise.
        if (total == 1 || total % 1000 == 0)
        {
            _logger.LogWarning(
                "IndexSearch forward queue is full; dropped {Total} item(s) so far (most recent: {Target}). " +
                "The periodic full reindex will repair the resulting drift.",
                total,
                item.Target);
        }
    }
}
