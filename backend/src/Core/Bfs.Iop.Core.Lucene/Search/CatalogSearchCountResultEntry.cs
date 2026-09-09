namespace Bfs.Iop.Core.Lucene.Search;

public sealed record CatalogSearchCountResultEntry
{
    public IReadOnlyDictionary<string, int> CountByValues { get; init; } = new Dictionary<string, int>().AsReadOnly();

    public required string Identifier { get; init; }

    public int TotalDocumentsCount { get; init; }
}
