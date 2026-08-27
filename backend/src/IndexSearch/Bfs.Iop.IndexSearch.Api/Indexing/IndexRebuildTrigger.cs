namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Why the background builder woke up.
/// </summary>
/// <param name="Requested">
/// True if an HTTP request asked for a rebuild, false if the schedule elapsed first.
/// </param>
/// <param name="Recreate">
/// True if any request since the last wake asked for the indexes to be dropped first. Independent of
/// <paramref name="Requested"/>, so a caller cannot lose a recreate by racing the schedule.
/// </param>
public readonly record struct IndexRebuildWakeUp(bool Requested, bool Recreate);

/// <summary>
/// Lets an HTTP request ask the background builder for an out-of-schedule full rebuild.
/// </summary>
public interface IIndexRebuildTrigger
{
    /// <summary>Requests a rebuild. Repeated calls before one starts collapse into a single run.</summary>
    /// <param name="recreate">
    /// Drop and recreate both indexes before rebuilding, purging documents whose rows no longer
    /// exist and applying any mapping change. Destructive: searches return no results between the
    /// drop and the end of the build.
    /// </param>
    void Request(bool recreate);

    /// <summary>Waits until a rebuild is requested or the interval elapses.</summary>
    Task<IndexRebuildWakeUp> WaitForRequestAsync(TimeSpan interval, CancellationToken cancellationToken);
}

/// <summary>
/// A single reset event rather than a queue: the operation is "rebuild everything", so ten requests
/// arriving together still mean one rebuild. The builder consumes the signal when it wakes.
/// <para>
/// <b>Coalescing is strongest-wins, not last-wins.</b> If any request in a collapsed group asked to
/// recreate, the single run that results recreates. Last-wins would let
/// <c>Request(recreate: true)</c> followed by <c>Request(recreate: false)</c> answer 202 and then
/// purge nothing — success reported, nothing done, which is the failure this flag exists to prevent.
/// </para>
/// </summary>
internal sealed class IndexRebuildTrigger : IIndexRebuildTrigger
{
    private readonly SemaphoreSlim _signal = new(0, 1);

    /// <summary>1 once a recreate has been asked for, until the builder consumes it. Never cleared by
    /// a non-recreating request — see the strongest-wins note on the class.</summary>
    private int _recreateRequested;

    public void Request(bool recreate)
    {
        if (recreate)
        {
            // Set BEFORE signalling. The other order lets a builder woken by the signal read the flag
            // before it is written, turning a requested recreate into a plain rebuild.
            Interlocked.Exchange(ref _recreateRequested, 1);
        }

        // Already signalled: nothing to add, the pending rebuild will cover this request too.
        if (_signal.CurrentCount == 0)
        {
            try
            {
                _signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // Raced with another request that released first; one pending signal is enough.
            }
        }
    }

    public async Task<IndexRebuildWakeUp> WaitForRequestAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        bool requested;

        try
        {
            requested = await _signal.WaitAsync(interval, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            requested = false;
        }

        // Read and reset on every wake, including one caused by the schedule rather than a request.
        // A recreate arriving in the instant between the wait returning and this line leaves its
        // signal pending, so the loop runs once more immediately without recreating. That extra pass
        // is harmless — the build is idempotent — and is much easier to reason about than making the
        // two reads atomic.
        var recreate = Interlocked.Exchange(ref _recreateRequested, 0) == 1;

        return new IndexRebuildWakeUp(requested, recreate);
    }
}
