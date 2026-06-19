using Bfs.Iop.Core.Lucene.Index;

namespace Bfs.Iop.Core.Lucene.IndexBuilders;

internal sealed class LuceneIndexBuilderService
{
    private readonly ICodeListEntryIndexService _codeListEntryLuceneService;
    private readonly IEnumerable<IIndexBuilderService> _indexBuilderServices;

    public LuceneIndexBuilderService(
        ICodeListEntryIndexService codeListEntryLuceneService,
        IEnumerable<IIndexBuilderService> indexBuilderServices)
    {
        _codeListEntryLuceneService = codeListEntryLuceneService;
        _indexBuilderServices = indexBuilderServices;
    }

    public async Task RebuildIndex(CancellationToken cancellationToken = default)
    {
        // Unfortunately it is too dangerous to build the indexes in parallel,
        // because concurrent calls by the dbContext in the database may happen.

        foreach (var indexBuilder in _indexBuilderServices)
        {
            await indexBuilder.BuildIndex(cancellationToken);
        }

        await _codeListEntryLuceneService.BuildIndex(cancellationToken);
    }
}