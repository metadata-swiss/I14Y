using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

public sealed class IndexNames
{
    private readonly ElasticsearchOptions _options;

    public IndexNames(IOptions<ElasticsearchOptions> options) => _options = options.Value;

    public string Catalog => _options.CatalogIndexName;

    public string CodeList => _options.CodeListIndexName;
}
