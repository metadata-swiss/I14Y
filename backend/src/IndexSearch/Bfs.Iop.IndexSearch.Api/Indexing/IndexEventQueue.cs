using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <inheritdoc cref="IIndexEventQueue"/>
internal sealed class IndexEventQueue : IIndexEventQueue
{
    private readonly Channel<IndexEvent> _channel;
    private int _count;

    public IndexEventQueue(IOptions<IndexSearchOptions> options)
    {
        var capacity = Math.Max(1, options.Value.QueueCapacity);

        // FullMode.Wait would block the request thread; we want an immediate "full" answer so the
        // trigger endpoint can return 429 and the caller's circuit breaker can react.
        _channel = Channel.CreateBounded<IndexEvent>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public int Count => Volatile.Read(ref _count);

    public bool TryEnqueue(IndexEvent indexEvent)
    {
        if (!_channel.Writer.TryWrite(indexEvent))
        {
            return false;
        }

        Interlocked.Increment(ref _count);
        return true;
    }

    public async ValueTask<IReadOnlyCollection<IndexEvent>> DequeueBatchAsync(int maxItems, CancellationToken cancellationToken)
    {
        // Block until at least one event is available, then drain whatever else is already queued.
        //
        // Draining is what MAKES collapsing possible; it does not collapse anything itself. The
        // batch is handed to IndexEventProcessor, which is where repeats to the same (Target, Id)
        // are folded into one reconcile.
        var first = await _channel.Reader.ReadAsync(cancellationToken);
        Interlocked.Decrement(ref _count);

        var batch = new List<IndexEvent>(Math.Min(maxItems, 64)) { first };

        while (batch.Count < maxItems && _channel.Reader.TryRead(out var next))
        {
            Interlocked.Decrement(ref _count);
            batch.Add(next);
        }

        return batch;
    }
}
