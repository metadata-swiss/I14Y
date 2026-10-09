using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Http;

namespace Bfs.Iop.IndexSearch.ApiClient.Authentication;

public sealed class RequestUserTokenRetriever : ITokenRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestUserTokenRetriever(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public Task<string> GetAuthTokenAsync() => RequestBearerToken.ReadAsync(_httpContextAccessor);
}
