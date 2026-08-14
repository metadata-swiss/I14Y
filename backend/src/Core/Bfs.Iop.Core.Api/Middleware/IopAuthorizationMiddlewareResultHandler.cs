using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Middleware;

public class IopAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IUserContextService _userContextService;
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public IopAuthorizationMiddlewareResultHandler(IUserContextService userContextService) =>
        _userContextService = userContextService;

    public async Task HandleAsync(
        RequestDelegate next, 
        HttpContext context, 
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // Check if the token is authorized and valid (contains a valid business role).
        // If not, return a 403 Forbidden response.
        if (authorizeResult.Succeeded && !_userContextService.IsUserTokenValid())
        {
            context.Response.StatusCode = 403;
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);   
    }
}
