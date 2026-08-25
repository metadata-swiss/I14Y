using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.Middleware;

internal sealed class RegisterUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RegisterUserMiddleware> _logger;

    public RegisterUserMiddleware(RequestDelegate next, ILogger<RegisterUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIopCoreApiClient apiClient)
    {
        var authResult = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult;

        if (authResult?.Succeeded ?? false)
        {
            try
            {
                await apiClient.GetPersonsSelfRegisteredAsync(default);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error occurred while registering/updating logged user.");
            }
        }

        await _next(context);
    }
}