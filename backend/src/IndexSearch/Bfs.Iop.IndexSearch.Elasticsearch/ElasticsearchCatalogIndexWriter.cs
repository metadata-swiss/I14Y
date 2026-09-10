using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class ElasticsearchCatalogIndexWriter : ICatalogIndexWriter
{
    private readonly ElasticsearchBulkWriter _bulk;
    private readonly IndexWriteTarget _target;

    public ElasticsearchCatalogIndexWriter(ElasticsearchBulkWriter bulk, IndexWriteTarget target)
    {
        _bulk = bulk;
        _target = target;
    }

    public Task<int> WriteAsync(
        IReadOnlyCollection<CatalogIndexDocument> documents,
        CancellationToken cancellationToken = default) =>
        _bulk.IndexAsync(
            _target.Catalog,
            [.. documents.Select(CatalogDocumentFactory.Build)],
            cancellationToken);

    public Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        _bulk.DeleteAsync(_target.Catalog, ids, cancellationToken);
}
