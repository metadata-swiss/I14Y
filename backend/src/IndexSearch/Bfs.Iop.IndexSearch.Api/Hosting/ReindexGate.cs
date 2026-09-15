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

        // The previous pass's counts go too. A rebuilder only reports once it has finished, so keeping
        // them would show a run that started microseconds ago as having already written every
        // document, and a run that later failed as having written them all.
        Volatile.Write(ref _status, Status with
        {
            IsRunning = true,
            Operation = operation,
            StartedAt = _time.GetUtcNow(),
            FinishedAt = null,
            LastSucceeded = null,
            CatalogReport = null,
            CodeListReport = null,
        });

        return true;
    }

    // Whatever this pass produced, including nothing. A pass that failed before a rebuilder finished
    // reports no counts rather than the previous run's, which would read as documents this run wrote.
    public void End(bool succeeded, IndexRebuildReport? catalog = null, IndexRebuildReport? codeLists = null)
    {
        Volatile.Write(ref _status, Status with
        {
            IsRunning = false,
            FinishedAt = _time.GetUtcNow(),
            LastSucceeded = succeeded,
            CatalogReport = catalog,
            CodeListReport = codeLists,
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