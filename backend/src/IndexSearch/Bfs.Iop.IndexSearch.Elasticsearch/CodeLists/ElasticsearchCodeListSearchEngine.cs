using Bfs.Iop.IndexSearch.Contracts.Search;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListSearchEngine : ICodeListSearchEngine
{
    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;

    public ElasticsearchCodeListSearchEngine(ElasticsearchSearchExecutor executor, IndexNames names)
    {
        _executor = executor;
        _names = names;
    }

    public async Task<PagedResult<CodeListSearchHit>> SearchAsync(
        Guid conceptId,
        string? query,
        string language,
        CodeListSearchFilter? filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (from, size) = Paging.ToWindow(page, pageSize);

        var body = CodeListQueryBuilder.BuildSearchBody(conceptId, query, language, filter, from, size);

        using var response = await _executor.SearchAsync(_names.CodeList, body, cancellationToken);

        return CodeListResponseReader.ReadSearch(response.RootElement, page, pageSize);
    }
}
