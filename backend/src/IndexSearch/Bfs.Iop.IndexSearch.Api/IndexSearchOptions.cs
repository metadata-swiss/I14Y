namespace Bfs.Iop.IndexSearch.Api;

public sealed class IndexSearchOptions
{
    public const string SectionName = "IndexSearch";
    public int ReindexBatchSize { get; set; } = 1000;
}
