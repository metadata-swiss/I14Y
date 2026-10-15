namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

/// <summary>
///     The node the integration fixtures talk to, or a skip.
/// </summary>
/// <remarks>
///     Without this a developer with no container running sees these fail rather than skip, which is
///     two minutes of red that says nothing about their change.
/// </remarks>
internal static class LiveElasticsearch
{
    private const string Default = "http://localhost:9200";

    public static string RequireLocalNode()
    {
        var uri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH") ?? Default;

        // These fixtures delete indices, so a remote node is refused rather than skipped: it means the
        // variable points somewhere it should not, and that is worth failing over.
        if (new Uri(uri).Host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail($"Refusing to run against '{new Uri(uri).Host}': these fixtures delete indices.");
        }

        using var client = new HttpClient { BaseAddress = new Uri(uri), Timeout = TimeSpan.FromSeconds(3) };

        try
        {
            client.GetAsync("/").GetAwaiter().GetResult().EnsureSuccessStatusCode();
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            Assert.Ignore($"No Elasticsearch at {uri}. Start one with: docker compose up -d elasticsearch");
        }

        return uri;
    }
}
