using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Bfs.Iop.Infrastructure.Security.Helpers;

public static class RequestBearerToken
{
    private const string Scheme = "Bearer ";

    public static async Task<string> ReadAsync(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        var context = httpContextAccessor.HttpContext;

        if (context is null)
        {
            return string.Empty;
        }

        var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();

        if (!string.IsNullOrWhiteSpace(authorization)
            && authorization.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return authorization[Scheme.Length..].Trim();
        }

        return await context.GetTokenAsync("access_token") ?? string.Empty;
    }
}
