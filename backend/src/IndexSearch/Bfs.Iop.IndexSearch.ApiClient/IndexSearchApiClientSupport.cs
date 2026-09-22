using Bfs.Iop.Infrastructure.ApiClient.Extensions;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

namespace Bfs.Iop.IndexSearch.ApiClient;

internal class IndexSearchApiClientSupport : INewtonsoftJsonClientSupport
{
    private readonly string _baseAddress;
    private readonly ITokenRetriever? _tokenRetriever;

    public IndexSearchApiClientSupport(string baseAddress, ITokenRetriever? tokenRetriever)
    {
        _baseAddress = baseAddress;
        _tokenRetriever = tokenRetriever;
    }

    public void SetupApiClientOptions(INewtonsoftClientOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder.SetBaseUrl(_baseAddress);

        if (_tokenRetriever is not null)
        {
            optionsBuilder.AddBearerAuthentication(() => _tokenRetriever.GetAuthTokenAsync());
        }
    }
}
