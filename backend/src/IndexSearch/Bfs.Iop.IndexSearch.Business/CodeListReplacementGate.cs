using System.Collections.Concurrent;

namespace Bfs.Iop.IndexSearch.Business;


public sealed class CodeListReplacementGate
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();

    public async Task<IDisposable> EnterAsync(Guid conceptId, CancellationToken cancellationToken = default)
    {
        var gate = _gates.GetOrAdd(conceptId, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(cancellationToken);

        return new Exit(gate);
    }

    private sealed class Exit : IDisposable
    {
        private readonly SemaphoreSlim _gate;

        private int _released;

        public Exit(SemaphoreSlim gate) => _gate = gate;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                _gate.Release();
            }
        }
    }
}
