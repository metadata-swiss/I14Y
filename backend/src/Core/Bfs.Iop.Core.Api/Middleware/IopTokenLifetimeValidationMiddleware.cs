using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Middleware;

public class IopTokenLifetimeValidationMiddleware
{
    private readonly RequestDelegate _next;

    public IopTokenLifetimeValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if a token has expired.
        // This check is also made for anonymous requests, as the token may be expired and the user may not have a valid token to refresh it.
        var authentication =
            await context.AuthenticateAsync("Bearer");

        if (authentication.Failure is SecurityTokenExpiredException)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append(
                HeaderNames.WWWAuthenticate,
                $"\"Bearer error=\"{authentication.Failure.Message}\"");

            return;
        }

        await _next(context);
    }
}