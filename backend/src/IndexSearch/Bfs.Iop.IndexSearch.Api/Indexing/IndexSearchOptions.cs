namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Settings of the IndexSearch service itself, bound from the "IndexSearch" configuration section.
/// (Connection details for Elasticsearch live in the separate "Elasticsearch" section.)
/// </summary>
public sealed class IndexSearchOptions
{
    public const string SectionName = "IndexSearch";

    /// <summary>
    /// Pre-shared secret required by the /api/Index write routes, supplied in the X-Api-Key header.
    /// When empty every call is rejected: an unconfigured secret must fail closed, never open.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>Maximum number of pending index events held in memory before callers get a 429.</summary>
    public int QueueCapacity { get; set; } = 10_000;

    /// <summary>Maximum number of events collapsed into a single reconcile pass.</summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// How often the whole index is rebuilt from the database. This is what repairs drift caused by
    /// a trigger that never arrived or a queue lost on restart, so it is required rather than
    /// optional. Set to 0 to disable (only sensible in local development).
    /// </summary>
    public int FullReindexIntervalHours { get; set; } = 6;

    /// <summary>Whether to run a full index build at startup once the index has been ensured.</summary>
    public bool BuildIndexOnStartup { get; set; } = true;
}
