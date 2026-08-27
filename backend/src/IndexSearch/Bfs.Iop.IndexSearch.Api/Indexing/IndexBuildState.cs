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

    /// <summary>
    /// Whether the last full build could read the dataset structures container. False means every
    /// dataset was indexed as structure-less, so the Structures facet is empty — surfaced here
    /// because the alternative is a filter that silently looks broken.
    /// </summary>
    bool? StructuresAvailable { get; }

    /// <summary>
    /// Declares the indexes no longer usable — call this the moment they are dropped.
    /// <para>
    /// <see cref="IsReady"/> latches on the first successful build and is never cleared by a
    /// failure, which is right for an ordinary rebuild: the previous documents are still there. It is
    /// wrong for a recreate, where the documents are gone before the rebuild starts. Without this,
    /// a recreate that fails after an earlier success reports <c>ready: true</c> over an empty index,
    /// and the readiness probe keeps the replica in rotation.
    /// </para>
    /// </summary>
    void MarkNotReady();

    /// <summary>Runs <paramref name="build"/> unless one is already in progress, in which case it waits.</summary>
    Task RunFullBuildAsync(Func<CancellationToken, Task<bool?>> build, CancellationToken cancellationToken);

    /// <summary>
    /// Runs <paramref name="work"/> under the same gate a full build takes, so single-document writes
    /// and a rebuild of the same index never overlap.
    /// <para>
    /// The gate is held for the duration of <paramref name="work"/> rather than merely checked before
    /// it. Checking is not enough: a writer that passed the check can still be mid-flight when the
    /// builder drops the index, and its bulk request then makes Elasticsearch auto-create the index
    /// with a dynamic mapping — no analyzers, no keyword sub-fields — which searches wrongly with
    /// nothing logged.
    /// </para>
    /// <para>
    /// Does <b>not</b> set <see cref="IsFullBuildRunning"/>: a reconcile is not a rebuild, and
    /// reporting one as the other would make the rebuild endpoints answer 409 for the wrong reason.
    /// </para>
    /// </summary>
    Task RunGatedAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken);
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

    /// <summary>0 = no build has finished yet, 1 = structures were readable, 2 = they were not.</summary>
    private int _structuresAvailable;

    // Written on the builder thread, read on request threads, so it needs the same barrier as its
    // neighbours above rather than a plain auto-property.
    public bool? StructuresAvailable => Volatile.Read(ref _structuresAvailable) switch
    {
        1 => true,
        2 => false,
        _ => null,
    };

    public async Task RunFullBuildAsync(Func<CancellationToken, Task<bool?>> build, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(build);

        await _gate.WaitAsync(cancellationToken);
        _running = true;

        try
        {
            var structuresAvailable = await build(cancellationToken);

            Volatile.Write(ref _structuresAvailable, structuresAvailable switch
            {
                true => 1,
                false => 2,
                null => 0,
            });

            Interlocked.Exchange(ref _lastCompletedTicks, DateTimeOffset.UtcNow.UtcTicks);
        }
        finally
        {
            _running = false;
            _gate.Release();
        }
    }

    public void MarkNotReady()
    {
        Interlocked.Exchange(ref _lastCompletedTicks, 0);

        // Cleared too: whether structures were readable described the documents that have just been
        // deleted. Reporting the old answer against a fresh index would outlive the thing it measured.
        Volatile.Write(ref _structuresAvailable, 0);
    }

    public async Task RunGatedAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);

        await _gate.WaitAsync(cancellationToken);

        try
        {
            await work(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
