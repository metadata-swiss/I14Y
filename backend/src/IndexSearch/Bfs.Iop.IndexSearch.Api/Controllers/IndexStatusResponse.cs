using Bfs.Iop.IndexSearch.Business;

namespace Bfs.Iop.IndexSearch.Api.Controllers;

public sealed record ReindexCounts(
    int DocumentsSent,
    int DocumentsWritten,
    int BatchesFailed,
    bool? StructuresResolved)
{
    internal static ReindexCounts? From(IndexRebuildReport? report) => report is null
        ? null
        : new ReindexCounts(
            report.DocumentsSent,
            report.DocumentsWritten,
            report.BatchesFailed,
            report.StructuresResolved);
}


public sealed record IndexStatusResponse(
    bool Running,
    string? Operation,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    TimeSpan? Elapsed,
    bool? LastSucceeded,
    ReindexCounts? Catalog,
    ReindexCounts? CodeLists);