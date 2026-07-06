using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authentication;
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

    public async Task<string> GetAuthTokenAsync()
    {
        string? token = null;
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            token = await context.GetTokenAsync("access_token");
        }
        return token ?? "";
    }
}