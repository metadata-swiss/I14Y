namespace Bfs.Iop.IndexSearch.Api;

public sealed class IndexSearchOptions
{
    public const string SectionName = "IndexSearch";
    public StartupAction OnStartup { get; set; } = StartupAction.CreateIndices;
    public int ReindexBatchSize { get; set; } = 1000;
    public bool ForceMergeAfterReindex { get; set; } = true;
}
