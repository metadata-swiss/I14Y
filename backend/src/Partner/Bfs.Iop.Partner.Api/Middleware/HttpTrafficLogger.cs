using System.Text;

namespace Bfs.Iop.Partner.Api.Middleware;

#pragma warning disable CS1591
public sealed class HttpTrafficLogger
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpTrafficLogger> _logger;

    public HttpTrafficLogger(
        RequestDelegate next,
        ILogger<HttpTrafficLogger> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var logDetails = _logger.IsEnabled(LogLevel.Debug);

        if (!logDetails)
        {
            await _next(context);

            _logger.LogInformation(
                "Request {RequestMethod} {RequestPath} => {StatusCode}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode);

            return;
        }

        context.Request.EnableBuffering();

        await LogRequestDetails(context);

        var originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);

            _logger.LogInformation(
                "Request {RequestMethod} {RequestPath} => {StatusCode}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode);

            await LogResponseDetails(context);

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task LogRequestDetails(HttpContext context)
    {
        try
        {
            var body = await ReadBodyAsync(context.Request.Body);

            var log = new StringBuilder()
                .AppendLine($"Request {context.Request.Method} {context.Request.Path}")
                .AppendLine(" Headers:");

            AppendHeaders(log, context.Request.Headers);

            log.AppendLine(" Body:")
               .AppendLine($"  {body}");

#pragma warning disable CA2254
            _logger.LogDebug(log.ToString());
#pragma warning restore CA2254
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log request details.");
        }
    }

    private async Task LogResponseDetails(HttpContext context)
    {
        try
        {
            var body = await ReadBodyAsync(context.Response.Body);

            var log = new StringBuilder()
                .AppendLine($"Response {context.Request.Method} {context.Request.Path}")
                .AppendLine(" Headers:");

            AppendHeaders(log, context.Response.Headers);

            log.AppendLine(" Body:")
               .AppendLine($"  {body}");

#pragma warning disable CA2254
            _logger.LogDebug(log.ToString());
#pragma warning restore CA2254
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log response details.");
        }
    }

    private static async Task<string> ReadBodyAsync(Stream stream)
    {
        stream.Position = 0;

        using var reader = new StreamReader(stream, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        stream.Position = 0;

        return body;
    }

    private static void AppendHeaders(
        StringBuilder builder,
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> headers)
    {
        foreach (var header in headers)
        {
            builder.Append("  ")
                   .Append(header.Key)
                   .Append(": ")
                   .AppendLine(header.Value);
        }
    }
}