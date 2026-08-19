namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Shared state of the index: whether a full rebuild is running, and when one last succeeded.
/// Also acts as the single-flight gate so a rebuild and the event worker never write concurrently.
/// </summary>
public interface IIndexBuildState
{
    bool IsFullBuildRunning { get; }

    DateTimeOffset? LastFullBuildCompletedAt { get; }

    /// <summary>True once a full build has completed, i.e. the index is usable for serving.</summary>
    bool IsReady { get; }

    /// <summary>Runs <paramref name="build"/> unless one is already in progress, in which case it waits.</summary>
    Task RunFullBuildAsync(Func<CancellationToken, Task> build, CancellationToken cancellationToken);

    /// <summary>Blocks while a full build is running.</summary>
    Task WaitWhileFullBuildRunningAsync(CancellationToken cancellationToken);
}

internal sealed class IndexBuildState : IIndexBuildState
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _running;
    private long _lastCompletedTicks;

    public bool IsFullBuildRunning => _running;

    public DateTimeOffset? LastFullBuildCompletedAt
    {
        get
        {
            var ticks = Interlocked.Read(ref _lastCompletedTicks);
            return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
        }
    }

    public bool IsReady => Interlocked.Read(ref _lastCompletedTicks) != 0;

    public async Task RunFullBuildAsync(Func<CancellationToken, Task> build, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(build);

        await _gate.WaitAsync(cancellationToken);
        _running = true;

        try
        {
            await build(cancellationToken);
            Interlocked.Exchange(ref _lastCompletedTicks, DateTimeOffset.UtcNow.UtcTicks);
        }
        finally
        {
            _running = false;
            _gate.Release();
        }
    }

    public async Task WaitWhileFullBuildRunningAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        _gate.Release();
    }
}
