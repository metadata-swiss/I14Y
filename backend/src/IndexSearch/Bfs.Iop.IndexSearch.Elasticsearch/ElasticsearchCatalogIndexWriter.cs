using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogIndexWriter : ICatalogIndexWriter
{
    private readonly ElasticsearchBulkWriter _bulk;
    private readonly IndexWriteTarget _target;
    private readonly CatalogScoring _scoring;

    public ElasticsearchCatalogIndexWriter(
        ElasticsearchBulkWriter bulk,
        IndexWriteTarget target,
        IOptions<SearchScoringOptions> scoring)
    {
        _bulk = bulk;
        _target = target;
        _scoring = scoring.Value.Catalog;
    }

    public Task<int> WriteAsync(
        IReadOnlyCollection<CatalogIndexDocument> documents,
        CancellationToken cancellationToken = default) =>
        _bulk.IndexAsync(
            _target.Catalog,
            [.. documents.Select(x => CatalogDocumentFactory.Build(x, _scoring))],
            cancellationToken);

    public Task<int> DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        _bulk.DeleteAsync(_target.Catalog, ids, cancellationToken);
}
