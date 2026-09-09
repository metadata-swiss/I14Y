using Bfs.Iop.IndexSearch.Business;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public sealed class ReindexGate : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly TimeProvider _time;

    public ReindexGate(TimeProvider? time = null) => _time = time ?? TimeProvider.System;

    public bool IsRunning { get; private set; }

    public string? Operation { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public bool? LastSucceeded { get; private set; }

    public IndexRebuildReport? LastCatalogReport { get; private set; }

    public IndexRebuildReport? LastCodeListReport { get; private set; }

    public TimeSpan? Elapsed => StartedAt is null
        ? null
        : (FinishedAt ?? _time.GetUtcNow()) - StartedAt.Value;

    // Taken before the work is detached, so a caller can answer "already running" without waiting for
    // it. Whoever wins this must call End, which is why the only two callers wrap it in try/finally.
    public bool TryBegin(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        if (!_gate.Wait(0))
        {
            return false;
        }

        IsRunning = true;
        Operation = operation;
        StartedAt = _time.GetUtcNow();
        FinishedAt = null;
        LastSucceeded = null;

        return true;
    }

    // Reports are kept when a pass does not produce them, so a failure leaves the last known good
    // counts visible next to LastSucceeded = false rather than blanking them.
    public void End(bool succeeded, IndexRebuildReport? catalog = null, IndexRebuildReport? codeLists = null)
    {
        LastCatalogReport = catalog ?? LastCatalogReport;
        LastCodeListReport = codeLists ?? LastCodeListReport;

        IsRunning = false;
        FinishedAt = _time.GetUtcNow();
        LastSucceeded = succeeded;

        _gate.Release();
    }

    public async Task<bool> TryRunAsync(
        string operation,
        Func<Task> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(work);

        cancellationToken.ThrowIfCancellationRequested();

        if (!TryBegin(operation))
        {
            return false;
        }

        var succeeded = false;

        try
        {
            await work();

            succeeded = true;
            return true;
        }
        finally
        {
            End(succeeded);
        }
    }

    public void Dispose() => _gate.Dispose();
}