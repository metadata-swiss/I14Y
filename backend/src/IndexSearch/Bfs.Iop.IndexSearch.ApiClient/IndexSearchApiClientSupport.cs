using Bfs.Iop.Infrastructure.ApiClient.Extensions;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

namespace Bfs.Iop.IndexSearch.ApiClient;

internal class IndexSearchApiClientSupport : INewtonsoftJsonClientSupport
{
    internal const string HttpClientName = "IndexSearch.ApiClient";

    private readonly string _baseAddress;
    private readonly ITokenRetriever? _tokenRetriever;
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexSearchApiClientSupport(
        string baseAddress,
        ITokenRetriever? tokenRetriever,
        IHttpClientFactory httpClientFactory)
    {
        _baseAddress = baseAddress;
        _tokenRetriever = tokenRetriever;
        _httpClientFactory = httpClientFactory;
    }

    public void SetupApiClientOptions(INewtonsoftClientOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder.SetBaseUrl(_baseAddress);

        // Without this the base class falls back to new HttpClient() per call, and the generated
        // methods dispose it afterwards — a fresh connection pool and a socket in TIME_WAIT for every
        // search. The factory pools the handler, so the per-call dispose is free.
        optionsBuilder.SetHttpClientFactory(() =>
            Task.FromResult(_httpClientFactory.CreateClient(HttpClientName)));

        // No retriever is valid: the search endpoints answer anonymously. The index ones will 401.
        if (_tokenRetriever is not null)
        {
            optionsBuilder.AddBearerAuthentication(() => _tokenRetriever.GetAuthTokenAsync());
        }
    }
}
