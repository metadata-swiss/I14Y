using System.Collections.Concurrent;
using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public enum PendingIndexTarget
{
    CatalogResource = 1,
    CodeList = 2,
}

public enum PendingIndexOperation
{
    Upsert = 1,
    Remove = 2,
}

public sealed record PendingIndexWrite(
    PendingIndexTarget Target,
    Guid Id,
    PendingIndexOperation Operation,
    SearchResourceType? ResourceType = null);

/// <summary>
///     Remembers the single-document writes that landed on indices a running rebuild is about to
///     replace, so they can be applied again once the rebuild has published.
/// </summary>
public sealed class PendingIndexWrites
{
    internal const int Capacity = 10_000;

    private readonly ConcurrentDictionary<(PendingIndexTarget Target, Guid Id), PendingIndexWrite> _entries = new();
    private readonly ILogger<PendingIndexWrites> _logger;

    private readonly Lock _door = new();

    private int _dropped;
    private bool _recording;

    public PendingIndexWrites(ILogger<PendingIndexWrites> logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public int Count => _entries.Count;

    public bool IsRecording
    {
        get
        {
            lock (_door)
            {
                return _recording;
            }
        }
    }

    public void StartRecording()
    {
        lock (_door)
        {
            _recording = true;
        }
    }

    public void StopRecording()
    {
        lock (_door)
        {
            _recording = false;
        }
    }

    public bool Record(PendingIndexWrite write)
    {
        ArgumentNullException.ThrowIfNull(write);

        lock (_door)
        {
            return _recording && Remember(write);
        }
    }

    private bool Remember(PendingIndexWrite write)
    {
        var key = (write.Target, write.Id);

        // Capacity is checked against new keys only, so a busy resource updated a thousand times still
        // occupies one slot.
        if (!_entries.ContainsKey(key) && _entries.Count >= Capacity)
        {
            if (Interlocked.Exchange(ref _dropped, 1) == 0)
            {
                _logger.LogError(
                    "More than {Capacity} resources changed during this rebuild, so some changes will "
                    + "not be replayed when it publishes and the index will stay behind the database "
                    + "for them until the next rebuild.",
                    Capacity);
            }

            return false;
        }

        _entries[key] = write;

        return true;
    }

    public IReadOnlyCollection<PendingIndexWrite> Drain()
    {
        lock (_door)
        {
            return DrainEntries();
        }
    }

    private IReadOnlyCollection<PendingIndexWrite> DrainEntries()
    {
        var drained = new List<PendingIndexWrite>(_entries.Count);

        foreach (var key in _entries.Keys)
        {
            if (_entries.TryRemove(key, out var write))
            {
                drained.Add(write);
            }
        }

        Interlocked.Exchange(ref _dropped, 0);

        return drained;
    }
}
