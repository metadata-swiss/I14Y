using System.Security.Cryptography;
using System.Text;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Filters;

/// <summary>
/// Requires the pre-shared secret on the /api/Index write routes.
/// <para>
/// The secret travels in the <c>X-Api-Key</c> header rather than a query parameter: query strings
/// end up in access logs, proxy logs and browser history. That header name is also already on the
/// redaction list of <c>HttpTrafficLogger</c>, so it stays out of our own request logs for free.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IndexApiSecretAttribute : Attribute, IAsyncActionFilter
{
    internal const string HeaderName = "X-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var configured = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<IndexSearchOptions>>().Value.Secret;

        // Fail closed: an unconfigured secret must never mean "everyone may reindex".
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new ForbiddenException("The index API secret is not configured.");
        }

        var supplied = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(supplied) || !FixedTimeEquals(supplied, configured))
        {
            throw new ForbiddenException("A valid index API secret is required.");
        }

        await next();
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b));
}
