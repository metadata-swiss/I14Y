using Bfs.Iop.IndexSearch.Business;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public sealed class ReindexGate : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly TimeProvider _time;

    public ReindexGate(TimeProvider? time = null) => _time = time ?? TimeProvider.System;

    private ReindexStatus _status = ReindexStatus.Idle;

    // Read once, so everything a caller derives from it describes the same moment.
    public ReindexStatus Status => Volatile.Read(ref _status);

    public bool IsRunning => Status.IsRunning;

    public string? Operation => Status.Operation;

    public bool? LastSucceeded => Status.LastSucceeded;

    public TimeSpan? Elapsed => Status.ElapsedAt(_time.GetUtcNow());

    public bool TryBegin(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        if (!_gate.Wait(0))
        {
            return false;
        }

        Volatile.Write(ref _status, Status with
        {
            IsRunning = true,
            Operation = operation,
            StartedAt = _time.GetUtcNow(),
            FinishedAt = null,
            LastSucceeded = null,
        });

        return true;
    }

    // Reports are kept when a pass does not produce them, so a failure leaves the last known good
    // counts visible next to LastSucceeded = false rather than blanking them.
    public void End(bool succeeded, IndexRebuildReport? catalog = null, IndexRebuildReport? codeLists = null)
    {
        var previous = Status;

        Volatile.Write(ref _status, previous with
        {
            IsRunning = false,
            FinishedAt = _time.GetUtcNow(),
            LastSucceeded = succeeded,
            CatalogReport = catalog ?? previous.CatalogReport,
            CodeListReport = codeLists ?? previous.CodeListReport,
        });

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