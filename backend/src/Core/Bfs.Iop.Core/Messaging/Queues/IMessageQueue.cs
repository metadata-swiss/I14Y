namespace Bfs.Iop.Core.Messaging.Queues;

public interface IMessageQueue<T> where T: class
{
    ValueTask EnqueueAsync(T message, CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> DequeueAllAsync(CancellationToken cancellationToken = default);
}
