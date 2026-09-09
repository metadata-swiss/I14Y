namespace Bfs.Iop.IndexSearch.Api;

public sealed class IndexSearchOptions
{
    public const string SectionName = "IndexSearch";
    public string? ApiKey { get; set; }
    public bool CreateIndicesOnStartup { get; set; } = true;
    public bool ReindexOnStartup { get; set; }
    public bool ResetOnStartup { get; set; }
    public int ReindexBatchSize { get; set; } = 1000;
    public bool ForceMergeAfterReindex { get; set; } = true;
    public int MaxPageSize { get; set; } = 200;
}
