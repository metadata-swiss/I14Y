using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Http;

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
    public Task<string> GetAuthTokenAsync() => RequestBearerToken.ReadAsync(_httpContextAccessor);
}
