using Bfs.Iop.IndexSearch.ApiClient.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Authentication;

/// <summary>
/// Supplies the caller's access token to the IndexSearch read client, so a search answered by the
/// IndexSearch service is scoped to the same user it would have been when the index was in-process.
/// <para>
/// Core is the gateway: a token that reaches Core must reach IndexSearch too, or the search silently
/// narrows to public-only. That is a correctness bug the user sees as missing data, never as an
/// error, so it is worth being explicit about.
/// </para>
/// <para>
/// Read per call and never cached in a field: this provider is held by a pooled
/// <c>DelegatingHandler</c> that <c>IHttpClientFactory</c> reuses across requests and across users,
/// regardless of the DI lifetime registered for it. Caching the token here would serve one arbitrary
/// user's visibility to everyone. <see cref="IHttpContextAccessor"/> is safe because it is a
/// singleton over an <c>AsyncLocal</c>, resolved at call time.
/// </para>
/// </summary>
public sealed class IndexSearchRequestUserTokenProvider : IIndexSearchTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexSearchRequestUserTokenProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public async Task<string?> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is null)
        {
            // No HTTP context means no caller — a background worker, or the client generator. An
            // anonymous query is the correct answer here, not an error.
            return null;
        }

        return await context.GetTokenAsync("access_token");
    }
}
