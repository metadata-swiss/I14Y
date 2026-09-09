namespace Bfs.Iop.IndexSearch.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Uri { get; set; } = "http://localhost:9200";

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string CatalogIndexName { get; set; } = "catalog";

    public string CodeListIndexName { get; set; } = "codelist";
}
