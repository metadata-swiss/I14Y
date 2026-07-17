namespace Bfs.Iop.Core.Settings;

/// <summary>
/// Connection + index settings for the Elasticsearch search PoC.
/// Bound from the "Elasticsearch" configuration section.
/// </summary>
public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    /// <summary>Base URL of the Elasticsearch node, e.g. http://localhost:9200 (see docker-compose.yml).</summary>
    public string Uri { get; set; } = "http://localhost:9200";

    /// <summary>Name of the catalog index.</summary>
    public string CatalogIndexName { get; set; } = "catalog";

    /// <summary>Name of the codelist-entry index.</summary>
    public string CodeListIndexName { get; set; } = "codelist";

    /// <summary>Optional basic-auth user (leave empty for the local security-disabled dev node).</summary>
    public string? Username { get; set; }

    /// <summary>Optional basic-auth password.</summary>
    public string? Password { get; set; }

    /// <summary>When true, drop and recreate the index on startup before (re)building it.</summary>
    public bool RecreateIndexOnStartup { get; set; } = true;
}
