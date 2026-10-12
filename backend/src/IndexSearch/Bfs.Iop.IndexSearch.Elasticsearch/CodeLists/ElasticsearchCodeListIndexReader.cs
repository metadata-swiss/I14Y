using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListIndexReader : ICodeListIndexReader
{
    private const int MaxEntries = 50_000;

    private readonly ElasticsearchSearchExecutor _executor;
    private readonly IndexWriteTarget _target;

    public ElasticsearchCodeListIndexReader(ElasticsearchSearchExecutor executor, IndexWriteTarget target)
    {
        _executor = executor;
        _target = target;
    }

    public async Task<IReadOnlyCollection<Guid>> ReadEntryIdsAsync(
        Guid conceptId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["size"] = MaxEntries,
            ["_source"] = false,
            ["track_total_hits"] = false,
            ["query"] = new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?>
                {
                    [EsCodeListFields.ConceptId] = conceptId.ToString(),
                },
            },
        };

        using var response = await _executor.SearchAsync(_target.CodeList, body, cancellationToken);

        return
        [
            .. response.RootElement
                .GetProperty("hits")
                .GetProperty("hits")
                .EnumerateArray()
                .Select(x => x.GetProperty("_id").GetString())
                .Where(x => Guid.TryParse(x, out _))
                .Select(Guid.Parse!),
        ];
    }
}
