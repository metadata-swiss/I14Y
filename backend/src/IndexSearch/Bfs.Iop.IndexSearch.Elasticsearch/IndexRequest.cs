namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed record IndexRequest(string Id, Dictionary<string, object?> Body);
