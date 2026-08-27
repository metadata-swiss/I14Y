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

    /// <summary>
    /// Whether the scheduled rebuild also drops and recreates the indexes first. Defaults to
    /// <c>false</c>.
    /// <para>
    /// It decides which of two flaws you live with, so neither default is free:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// <b>Off (default).</b> A rebuild is upsert-only, so a document whose row was deleted in
    /// Postgres — and whose de-index event was lost, which the in-memory forward queue makes routine
    /// — stays searchable until the service restarts. A mapping change likewise waits for a restart.
    /// </description></item>
    /// <item><description>
    /// <b>On.</b> Both heal within <see cref="FullReindexIntervalHours"/>, at the cost of an
    /// empty-results window every interval, unattended, on every replica.
    /// </description></item>
    /// </list>
    /// <para>
    /// Off is the default because the window is recurring and silent, whereas the stale document is
    /// repaired by any restart and by <c>POST /api/Index/recreate</c> on demand. Turn it on where
    /// deletions are frequent enough that waiting for a restart is worse.
    /// </para>
    /// </summary>
    public bool RecreateOnScheduledRebuild { get; set; }
}
