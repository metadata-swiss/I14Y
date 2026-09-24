using System.Net.Http.Headers;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;
using Newtonsoft.Json;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

// The support class is the only place the client decides how it reaches IndexSearch: which address,
// whether a token goes with the request, and whether the connection pool is reused. None of that is
// visible from the generated code, and all three fail quietly rather than loudly.
[TestFixture]
internal sealed class IndexSearchApiClientSupportTests
{
    private const string BaseAddress = "http://indexsearch.test";

    [Test]
    public async Task The_base_address_reaches_the_request()
    {
        var builder = Configure(Support());

        var client = await builder.ApplyAsync();

        client.BaseAddress.Should().Be(new Uri(BaseAddress));
    }

    [Test]
    public void The_http_client_comes_from_the_factory_rather_than_a_new_one_per_call()
    {
        // Without a factory the base class news up an HttpClient per call and the generated methods
        // dispose it, so every search builds and tears down its own connection pool. Under load that
        // is socket exhaustion, and nothing about it is visible until it happens.
        var builder = Configure(Support());

        builder.HttpClientFactory.Should().NotBeNull("the client would otherwise be created per call");
    }

    [Test]
    public async Task The_factory_hands_back_the_pooled_client()
    {
        var factory = new RecordingHttpClientFactory();
        var builder = Configure(Support(httpClientFactory: factory));

        var produced = await builder.HttpClientFactory!.Invoke();

        produced.Should().BeSameAs(factory.Client);
        factory.RequestedNames.Should().Equal("IndexSearch.ApiClient");
    }

    [Test]
    public async Task A_client_with_no_token_retriever_sends_no_authorization_header()
    {
        // Valid, not broken: the search endpoints answer anonymously and return the public corpus.
        var builder = Configure(Support(tokenRetriever: null));

        var request = await builder.ApplyToRequestAsync();

        request.Headers.Authorization.Should().BeNull();
    }

    [Test]
    public async Task A_token_retriever_puts_its_token_on_the_request()
    {
        var builder = Configure(Support(tokenRetriever: new StubTokenRetriever("a-token")));

        var request = await builder.ApplyToRequestAsync();

        request.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Bearer", "a-token"));
    }

    [Test]
    public async Task An_empty_token_is_not_sent_as_a_header()
    {
        // What the request-forwarding retrievers return when there is no incoming token — Admin's and
        // Partner's both end in `return token ?? ""`. Sending "Bearer " would be worse than sending
        // nothing: the server would try to parse it rather than treat the caller as anonymous.
        var builder = Configure(Support(tokenRetriever: new StubTokenRetriever(string.Empty)));

        var request = await builder.ApplyToRequestAsync();

        request.Headers.Authorization.Should().BeNull();
    }

    private static IndexSearchApiClientSupport Support(
        ITokenRetriever? tokenRetriever = null,
        IHttpClientFactory? httpClientFactory = null) =>
        new(BaseAddress, tokenRetriever, httpClientFactory ?? new RecordingHttpClientFactory());

    private static CapturingOptionsBuilder Configure(IndexSearchApiClientSupport support)
    {
        var builder = new CapturingOptionsBuilder();

        support.SetupApiClientOptions(builder);

        return builder;
    }

    /// <summary>
    ///     The real builder is internal to the infrastructure assembly, so this stands in for it and
    ///     keeps what was registered. The interface it implements is the same one the support writes
    ///     to, and replaying the captured delegates is what turns "a callback was registered" into an
    ///     assertion about the request that actually goes out.
    /// </summary>
    private sealed class CapturingOptionsBuilder : INewtonsoftClientOptionsBuilder
    {
        private readonly List<Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task>> _prepare = [];

        public Func<Task<HttpClient>>? HttpClientFactory { get; private set; }

        public async Task<HttpClient> ApplyAsync()
        {
            var client = new HttpClient();

            using var request = new HttpRequestMessage();

            foreach (var prepare in _prepare)
            {
                await prepare(client, request, string.Empty, CancellationToken.None);
            }

            return client;
        }

        public async Task<HttpRequestMessage> ApplyToRequestAsync()
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage();

            foreach (var prepare in _prepare)
            {
                await prepare(client, request, string.Empty, CancellationToken.None);
            }

            return request;
        }

        public void AddPrepareRequest(
            Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task> requestUpdater) =>
            _prepare.Add(requestUpdater);

        public void AddPrepareRequest(
            Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task> requestUpdater)
        {
        }

        public void AddProcessResponse(
            Func<HttpClient, HttpResponseMessage, CancellationToken, Task> responseProcessor)
        {
        }

        public void SetHttpClientFactory(Func<Task<HttpClient>> httpClientFactory) =>
            HttpClientFactory = httpClientFactory;

        public void AddUpdateJsonSerializerSettings(Action<JsonSerializerSettings> settingsUpdater)
        {
        }
    }

    private sealed class RecordingHttpClientFactory : IHttpClientFactory
    {
        public HttpClient Client { get; } = new();

        public List<string> RequestedNames { get; } = [];

        public HttpClient CreateClient(string name)
        {
            RequestedNames.Add(name);

            return Client;
        }
    }

    private sealed class StubTokenRetriever : ITokenRetriever
    {
        private readonly string _token;

        public StubTokenRetriever(string token) => _token = token;

        public Task<string> GetAuthTokenAsync() => Task.FromResult(_token);
    }
}
