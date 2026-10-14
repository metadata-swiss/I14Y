using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Authentication;

public class RequestUserTokenRetriever : ITokenRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestUserTokenRetriever(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public Task<string> GetAuthTokenAsync() => RequestBearerToken.ReadAsync(_httpContextAccessor);
}
