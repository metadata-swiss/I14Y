using Bfs.Iop.Infrastructure.Security.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bfs.Iop.Infrastructure.Security.Services;

internal sealed class AuthorizationProvider : IAuthorizationProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationProvider(
        IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public ClaimsPrincipal GetUser() => _httpContextAccessor.HttpContext?.User ?? new();
}
