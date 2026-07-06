using Bfs.Iop.Infrastructure.ApiClient.Extensions;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;
using System;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.ApiClient;

internal class BfsIopAdminApiClientSupport : INewtonsoftJsonClientSupport
{
    private readonly Func<Task<string>> _accessTokenFactory;
    private readonly string _baseUrl;

    public BfsIopAdminApiClientSupport(string baseUrl, Func<Task<string>> accessTokenFactory)
    {
        _baseUrl = baseUrl;
        _accessTokenFactory = accessTokenFactory;
    }

    public void SetupApiClientOptions(INewtonsoftClientOptionsBuilder optionsBuilder)
    {
        optionsBuilder.SetBaseUrl(_baseUrl);
        optionsBuilder.AddBearerAuthentication(_accessTokenFactory);
    }
}