namespace Bfs.Iop.Common.Messaging;

public interface IMessageQueue<T> where T: class
{
    ValueTask EnqueueAsync(T message, CancellationToken cancellationToken = default);

    IAsyncEnumerable<T> DequeueAllAsync(CancellationToken cancellationToken = default);
}
