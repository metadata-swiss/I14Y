using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Middleware
{
    public class HttpTrafficLogger
    {
        private readonly ILogger<HttpTrafficLogger> _logger;
        private readonly RequestDelegate _next;

        public HttpTrafficLogger(RequestDelegate next, ILogger<HttpTrafficLogger> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var detailsEnabled = _logger.IsEnabled(LogLevel.Debug);
            var originalResponseBody = context.Response.Body;
            if (detailsEnabled)
            {
                context.Request.EnableBuffering();
                await LogRequestDetails(context);

                context.Response.Body = new MemoryStream();
            }

            await _next(context);
            _logger.LogInformation("Request {requestMethod} {requestPathValue} => {responseStatusCode}", context.Request.Method, context.Request.Path.Value, context.Response.StatusCode);

            if (detailsEnabled)
            {
                await LogResponseDetails(context);

                await context.Response.Body.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }

        private async Task LogRequestDetails(HttpContext context)
        {
            try
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);

                var requestLog = new StringBuilder();
                requestLog.AppendLine($"Request {context.Request.Method} {context.Request.Path.Value}");
                requestLog.AppendLine(" Headers:");
                foreach (var item in context.Request.Headers)
                {
                    requestLog.AppendLine("  " + item.Key + ": " + item.Value);
                }
                requestLog.AppendLine(" Body:");
                requestLog.AppendLine("  " + requestBody);
#pragma warning disable CA2254 // Template should be a static expression
                _logger.LogDebug(requestLog.ToString());
#pragma warning restore CA2254 // Template should be a static expression
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falied to log request details");
            }
        }

        private async Task LogResponseDetails(HttpContext context)
        {
            try
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
                var responseBody = await reader.ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var responseLog = new StringBuilder();
                responseLog.AppendLine($"Response {context.Request.Method} {context.Request.Path.Value}");
                responseLog.AppendLine(" Headers:");
                foreach (var item in context.Response.Headers)
                {
                    responseLog.AppendLine("  " + item.Key + ": " + item.Value);
                }
                responseLog.AppendLine(" Body:");
                responseLog.AppendLine("  " + responseBody);
#pragma warning disable CA2254 // Template should be a static expression
                _logger.LogDebug(responseLog.ToString());
#pragma warning restore CA2254 // Template should be a static expression
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falied to log request details");
            }
        }
    }
}