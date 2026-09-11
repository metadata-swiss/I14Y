using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListIndexWriter : ICodeListIndexWriter
{
    private readonly ElasticsearchBulkWriter _bulk;
    private readonly IndexWriteTarget _target;

    public ElasticsearchCodeListIndexWriter(ElasticsearchBulkWriter bulk, IndexWriteTarget target)
    {
        _bulk = bulk;
        _target = target;
    }

    public Task<int> WriteAsync(
        IReadOnlyCollection<CodeListIndexDocument> documents,
        CancellationToken cancellationToken = default) =>
        _bulk.IndexAsync(
            _target.CodeList,
            [.. documents.Select(CodeListDocumentFactory.Build)],
            cancellationToken);

    public Task<int> DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        _bulk.DeleteAsync(_target.CodeList, ids, cancellationToken);
}
