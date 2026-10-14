using System.Text.Json;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal sealed class ElasticsearchCodeListIndexReader : ICodeListIndexReader
{
    private static readonly TimeSpan KeepAlive = TimeSpan.FromMinutes(2);

    private const int BatchSize = 5_000;

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
        var ids = new List<Guid>();

        var pit = await _executor.OpenPointInTimeAsync(_target.CodeList, KeepAlive, cancellationToken);

        try
        {
            IReadOnlyList<object>? cursor = null;

            while (true)
            {
                using var response = await _executor.SearchPointInTimeAsync(
                    Body(conceptId, pit, cursor),
                    cancellationToken);

                pit = CodeListResponseReader.ReadPointInTimeId(response.RootElement) ?? pit;

                var hits = response.RootElement.GetProperty("hits").GetProperty("hits");

                if (hits.GetArrayLength() == 0)
                {
                    return ids;
                }

                JsonElement last = default;

                foreach (var hit in hits.EnumerateArray())
                {
                    if (Guid.TryParse(hit.GetProperty("_id").GetString(), out var id))
                    {
                        ids.Add(id);
                    }

                    last = hit;
                }

                cursor = Cursor(last);

                // Reporting fewer entries than the index holds would leave the ones never seen behind.
                if (cursor.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Elasticsearch returned entries of the concept '{conceptId}' without a sort "
                        + "cursor, so the rest of them cannot be read.");
                }
            }
        }
        finally
        {
            await _executor.ClosePointInTimeAsync(pit, CancellationToken.None);
        }
    }

    private static Dictionary<string, object?> Body(
        Guid conceptId,
        string pointInTimeId,
        IReadOnlyList<object>? searchAfter)
    {
        var body = new Dictionary<string, object?>
        {
            ["size"] = BatchSize,
            ["_source"] = false,
            ["track_total_hits"] = false,
            ["query"] = new Dictionary<string, object?>
            {
                ["term"] = new Dictionary<string, object?>
                {
                    [EsCodeListFields.ConceptId] = conceptId.ToString(),
                },
            },
            ["sort"] = new object[] { new Dictionary<string, object?> { ["_shard_doc"] = "asc" } },
            ["pit"] = new Dictionary<string, object?>
            {
                ["id"] = pointInTimeId,
                ["keep_alive"] = $"{Math.Max(1, (int)Math.Ceiling(KeepAlive.TotalSeconds))}s",
            },
        };

        if (searchAfter is { Count: > 0 })
        {
            body["search_after"] = searchAfter;
        }

        return body;
    }

    private static IReadOnlyList<object> Cursor(JsonElement hit) =>
        hit.TryGetProperty("sort", out var sort) && sort.ValueKind == JsonValueKind.Array
            ? [.. sort.EnumerateArray().Select(Scalar)]
            : [];

    private static object Scalar(JsonElement value) =>
        value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var whole)
            ? whole
            : value.GetString() ?? string.Empty;
}
