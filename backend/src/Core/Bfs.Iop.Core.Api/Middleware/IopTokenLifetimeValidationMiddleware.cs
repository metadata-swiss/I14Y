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
        // For anonymous endpoints, still return 401 if the client sent an expired Bearer token (so it can be refreshed).
        var authResult = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult;

        if (authResult is null)
        {
            var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
            if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            authResult = await context.AuthenticateAsync("Bearer");
        }

        if (authResult.Failure is SecurityTokenExpiredException)
        {
            await context.ChallengeAsync("Bearer");
            return;
        }

        await _next(context);
    }
}