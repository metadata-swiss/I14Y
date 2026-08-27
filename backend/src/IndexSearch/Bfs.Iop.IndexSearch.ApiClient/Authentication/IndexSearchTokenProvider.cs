using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.ApiClient.Authentication;

/// <summary>
/// Supplies the access token to forward on read calls.
/// <para>
/// Deliberately its own abstraction rather than reusing <c>Bfs.Iop.Core.ApiClient.ITokenRetriever</c>:
/// this client must not depend on Core's client. Callers adapt whatever they already have — Admin
/// wraps its existing <c>RequestUserTokenRetriever</c> in one line.
/// </para>
/// </summary>
public interface IIndexSearchTokenProvider
{
    /// <summary>
    /// Returns the caller's access token, or null when the caller is anonymous.
    /// <para>
    /// <b>Implementations MUST read the token per call and MUST NOT capture it, or the
    /// <c>HttpContext</c> it came from, in a constructor or field.</b> Implementations are consumed by
    /// a <see cref="DelegatingHandler"/>, and <c>IHttpClientFactory</c> pools handler chains for the
    /// handler lifetime (two minutes by default) — so the instance behind this interface is shared
    /// across requests and across users, no matter what DI lifetime it is registered with. Capturing
    /// would pin one arbitrary user's token and serve their visibility to everyone, at HTTP 200, with
    /// no error anywhere. Resolve from <c>IHttpContextAccessor</c> at call time, which is a singleton
    /// over an <c>AsyncLocal</c> and therefore correct per request.
    /// </para>
    /// </summary>
    Task<string?> GetAccessTokenAsync();
}

/// <summary>
/// Attaches the caller's bearer token to every read request.
/// <para>
/// A <see cref="DelegatingHandler"/> rather than a default request header, because the token is
/// per-caller and <c>IHttpClientFactory</c> reuses handler instances across callers — a header set at
/// registration time would pin one user's token for everyone.
/// </para>
/// <para>
/// An anonymous caller is legitimate (search allows it) and simply gets no header, which yields
/// public-only results. What is NOT legitimate is a signed-in caller whose token silently fails to
/// arrive: results would quietly narrow to public-only with HTTP 200, which reads as missing data
/// rather than as an auth problem. Hence the warning below — it is the only trace such a bug leaves.
/// </para>
/// <para>
/// <b>This instance is reused across requests and across users.</b> <c>IHttpClientFactory</c> pools
/// the handler chain for the handler lifetime, so registering this as transient does not give it a
/// per-request instance, and <see cref="_tokenProvider"/> is resolved once per pooled chain rather
/// than once per request. That is safe only because the provider contract requires reading the token
/// per call — see <see cref="IIndexSearchTokenProvider.GetAccessTokenAsync"/>. Do not add
/// request-scoped state to this class.
/// </para>
/// </summary>
internal sealed class IndexSearchBearerTokenHandler : DelegatingHandler
{
    private readonly IIndexSearchTokenProvider _tokenProvider;
    private readonly ILogger<IndexSearchBearerTokenHandler> _logger;

    public IndexSearchBearerTokenHandler(
        IIndexSearchTokenProvider tokenProvider,
        ILogger<IndexSearchBearerTokenHandler> logger)
    {
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? token;

        try
        {
            token = await _tokenProvider.GetAccessTokenAsync();
        }
        catch (Exception ex)
        {
            // Never fail the search because token retrieval threw; degrade to anonymous and say so.
            _logger.LogWarning(ex, "Could not read the caller's access token; the IndexSearch request will be anonymous and return public-only results.");
            token = null;
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _logger.LogDebug("No access token available; querying IndexSearch anonymously (public-only results).");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
