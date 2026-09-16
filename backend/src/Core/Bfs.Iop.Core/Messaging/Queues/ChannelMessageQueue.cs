using System.Threading.Channels;

namespace Bfs.Iop.Core.Messaging.Queues;

internal sealed class ChannelMessageQueue<T> : IMessageQueue<T> where T: class
{
    public readonly Channel<T> _queue;

    public ChannelMessageQueue()
    {
        _queue = Channel.CreateBounded<T>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        });
    }

    public ValueTask EnqueueAsync(T message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        return _queue.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<T> DequeueAllAsync(CancellationToken cancellationToken = default) =>
        _queue.Reader.ReadAllAsync(cancellationToken);
}
