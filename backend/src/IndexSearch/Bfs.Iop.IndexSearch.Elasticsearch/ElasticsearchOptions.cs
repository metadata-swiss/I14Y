namespace Bfs.Iop.IndexSearch.Elasticsearch;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Uri { get; set; } = "http://localhost:9200";

    public string? Username { get; set; }

    public string? Password { get; set; }

    // No defaults: the deployed names are i14y-catalog and i14y-codelist, and a plausible-looking
    // fallback would quietly build and search a second, empty pair of indices instead of failing.
    public string CatalogIndexName { get; set; } = string.Empty;

    public string CodeListIndexName { get; set; } = string.Empty;
}
