using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Authentication;

public sealed class IndexSearchRequestUserTokenProvider : ITokenRetriever
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexSearchRequestUserTokenProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public Task<string> GetAuthTokenAsync() => RequestBearerToken.ReadAsync(_httpContextAccessor);
}
