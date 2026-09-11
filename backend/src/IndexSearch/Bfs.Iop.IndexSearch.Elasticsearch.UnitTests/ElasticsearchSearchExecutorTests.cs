using System.Net;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class ElasticsearchSearchExecutorTests
{
    [Test]
    public async Task The_body_reaches_the_node_as_the_search_request()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "{}");

        using var client = Client(handler);

        var executor = new ElasticsearchSearchExecutor(client);

        using var document = await executor.SearchAsync("i14y-codelist", Body(), CancellationToken.None);

        handler.Path.Should().Be("/i14y-codelist/_search");
        handler.Body.Should().Be("""{"size":3}""");
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task A_rejected_search_throws_with_what_the_node_said(HttpStatusCode status)
    {
        using var client = Client(status, """{"error":"failed to parse query"}""");

        var executor = new ElasticsearchSearchExecutor(client);

        var search = async () => await executor.SearchAsync("i14y-catalog", Body(), CancellationToken.None);

        // Elasticsearch explains a rejected query precisely; losing that turns a five-minute fix into
        // a guess.
        (await search.Should().ThrowAsync<HttpRequestException>())
            .WithMessage($"*{(int)status}*failed to parse query*");
    }

    private static Dictionary<string, object?> Body() => new() { ["size"] = 3 };

    private static HttpClient Client(HttpStatusCode status, string payload) =>
        Client(new StubHandler(status, payload));

    private static HttpClient Client(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("http://elasticsearch.test") };

    private sealed class StubHandler(HttpStatusCode status, string payload) : HttpMessageHandler
    {
        public string? Path { get; private set; }

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Path = request.RequestUri!.AbsolutePath;

            if (request.Content is not null)
            {
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(status)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
        }
    }
}
