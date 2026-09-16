using Bfs.Iop.IndexSearch.Business;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

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
