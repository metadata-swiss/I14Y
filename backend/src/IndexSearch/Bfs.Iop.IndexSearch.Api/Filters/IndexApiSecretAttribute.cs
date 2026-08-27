using System.Security.Cryptography;
using System.Text;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Mvc;
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
/// <para>
/// Rejections short-circuit with an explicit <see cref="StatusCodes.Status403Forbidden"/> result
/// rather than throwing: relying on exception-to-status mapping would make the answer depend on
/// middleware configuration, and a security check that silently degrades to 500 when that
/// configuration is missing is the wrong failure mode.
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

        var services = context.HttpContext.RequestServices;
        var configured = services.GetRequiredService<IOptions<IndexSearchOptions>>().Value.Secret;

        // Fail closed: an unconfigured secret must never mean "everyone may reindex".
        if (string.IsNullOrWhiteSpace(configured))
        {
            services.GetRequiredService<ILoggerFactory>()
                .CreateLogger<IndexApiSecretAttribute>()
                .LogError(
                    "IndexSearch:Secret is not configured, so every /api/Index call is rejected. " +
                    "Set it to the same value as IOP Core's IndexSearchClient:Secret.");

            context.Result = Forbidden("The index API secret is not configured.");
            return;
        }

        var supplied = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(supplied) || !FixedTimeEquals(supplied, configured))
        {
            context.Result = Forbidden("A valid index API secret is required.");
            return;
        }

        await next();
    }

    private static ObjectResult Forbidden(string detail) =>
        new(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = detail,
        })
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b));
}
