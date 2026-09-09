using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Filters;

public sealed class IndexApiKeyFilter : IAsyncActionFilter
{
    public const string HeaderName = "X-Index-Api-Key";

    private readonly IndexSearchOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<IndexApiKeyFilter> _logger;

    public IndexApiKeyFilter(
        IOptions<IndexSearchOptions> options,
        IHostEnvironment environment,
        ILogger<IndexApiKeyFilter> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!IsAuthorised(context))
        {
            _logger.LogWarning(
                "Rejected a call to {Path} without a valid {Header}.",
                context.HttpContext.Request.Path,
                HeaderName);

            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }

    private bool IsAuthorised(ActionExecutingContext context)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            if (_environment.IsDevelopment())
            {
                return true;
            }

            _logger.LogError(
                "IndexSearch:ApiKey is not configured, so every write endpoint is closed. Set it to "
                + "enable rebuilds in {Environment}.",
                _environment.EnvironmentName);

            return false;
        }

        var presented = context.HttpContext.Request.Headers[HeaderName].ToString();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(presented),
            Encoding.UTF8.GetBytes(_options.ApiKey));
    }
}
