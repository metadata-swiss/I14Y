namespace Bfs.Iop.IndexSearch.Api.Controllers;

public sealed record IndexStatusResponse(
    bool Running,
    string? Operation,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    TimeSpan? Elapsed,
    bool? LastSucceeded,
    ReindexCounts? Catalog,
    ReindexCounts? CodeLists);
