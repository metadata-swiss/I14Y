namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record IndexStatusResponse(
    bool Running,
    string? Operation,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    TimeSpan? Elapsed,
    bool? LastSucceeded,
    ReindexCounts? Catalog,
    ReindexCounts? CodeLists);
