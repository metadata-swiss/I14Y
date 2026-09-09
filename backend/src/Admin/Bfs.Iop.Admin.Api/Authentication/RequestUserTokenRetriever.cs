using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
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
            var authorizationHeader = context.Request.Headers[HeaderNames.Authorization].ToString();

            if (!string.IsNullOrWhiteSpace(authorizationHeader) &&
                authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorizationHeader.Substring("Bearer ".Length).Trim();
            }
            else
            {
                token = await context.GetTokenAsync("access_token");
            }
        }

        return token ?? "";
    }
}