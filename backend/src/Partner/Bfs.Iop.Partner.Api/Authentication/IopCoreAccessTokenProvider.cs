using Bfs.Iop.Core.ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using System;

namespace Bfs.Iop.Partner.Api.Authentication;

/// <summary>
/// Provides a way for passing through the token
/// </summary>
public class IopCoreAccessTokenProvider(IHttpContextAccessor httpContextAccessor) : ITokenRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Retrieves the auth token
    /// </summary>
    /// <returns></returns>
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
