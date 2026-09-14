using Bfs.Iop.IndexSearch.Contracts.Search;
using Microsoft.Extensions.Logging;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListSearchEngine : ICodeListSearchEngine
{
    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexNames _names;
    private readonly ILogger<ElasticsearchCodeListSearchEngine> _logger;

    public ElasticsearchCodeListSearchEngine(
        ElasticsearchSearchExecutor executor,
        IndexNames names,
        ILogger<ElasticsearchCodeListSearchEngine> logger)
    {
        _executor = executor;
        _names = names;
        _logger = logger;
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

        var result = CodeListResponseReader.ReadSearch(response.RootElement, page, pageSize, out var skipped);

        if (skipped > 0)
        {
            _logger.LogWarning(
                "Dropped {Skipped} of {Returned} entries from {Index}: their documents carry no "
                + "readable id or concept id. TotalCount still counts them.",
                skipped,
                skipped + result.Results.Count,
                _names.CodeList);
        }

        return result;
    }
}
