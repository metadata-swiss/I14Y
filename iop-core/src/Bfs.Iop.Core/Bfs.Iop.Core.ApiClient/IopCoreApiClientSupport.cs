using Bfs.Iop.Infrastructure.ApiClient.Extensions;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

namespace Bfs.Iop.Core.ApiClient;

internal class IopCoreApiClientSupport : INewtonsoftJsonClientSupport
{
    private readonly string _baseAddress;
    private readonly ITokenRetriever _tokenRetriever;

    public IopCoreApiClientSupport(string baseAddress, ITokenRetriever tokenRetriever)
    {
        _baseAddress = baseAddress;
        _tokenRetriever = tokenRetriever;
    }

    public void SetupApiClientOptions(INewtonsoftClientOptionsBuilder optionsBuilder)
    {
        optionsBuilder.SetBaseUrl(_baseAddress);

        if (_tokenRetriever is not null)
        {
            optionsBuilder.AddBearerAuthentication(() => _tokenRetriever.GetAuthTokenAsync());
        }
    }
}