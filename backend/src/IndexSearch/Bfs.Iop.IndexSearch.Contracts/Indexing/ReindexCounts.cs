namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record ReindexCounts(
    int DocumentsSent,
    int DocumentsWritten,
    int BatchesFailed,
    bool? StructuresResolved);
