using Bfs.Iop.IndexSearch.Business;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

// One value rather than seven fields. The pass updates this from a thread-pool thread while a status
// request reads it from another, so seven separate writes could be read half-applied: a poll landing
// between them saw IsRunning already false next to the previous pass's FinishedAt. A single reference,
// swapped in one write, cannot disagree with itself.
public sealed record ReindexStatus(
    bool IsRunning,
    string? Operation,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    bool? LastSucceeded,
    IndexRebuildReport? CatalogReport,
    IndexRebuildReport? CodeListReport)
{
    public static readonly ReindexStatus Idle = new(false, null, null, null, null, null, null);

    // A running pass has no FinishedAt yet, so it is measured against the caller's clock instead.
    public TimeSpan? ElapsedAt(DateTimeOffset now) =>
        StartedAt is null ? null : (FinishedAt ?? now) - StartedAt.Value;
}
