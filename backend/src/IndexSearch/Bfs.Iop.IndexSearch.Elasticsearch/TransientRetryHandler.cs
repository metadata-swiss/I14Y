using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal sealed class TransientRetryHandler : DelegatingHandler
{
    private const int MaxAttempts = 3;

    private readonly ILogger<TransientRetryHandler> _logger;
    private readonly TimeProvider _time;
    private readonly TimeSpan _baseDelay;

    public TransientRetryHandler(
        ILogger<TransientRetryHandler> logger,
        TimeProvider? time = null,
        TimeSpan? baseDelay = null)
    {
        _logger = logger;
        _time = time ?? TimeProvider.System;
        _baseDelay = baseDelay ?? TimeSpan.FromMilliseconds(200);
    }

    internal static bool IsTransient(HttpStatusCode status) => status switch
    {
        HttpStatusCode.RequestTimeout => true,
        HttpStatusCode.TooManyRequests => true,
        HttpStatusCode.BadGateway => true,
        HttpStatusCode.ServiceUnavailable => true,
        HttpStatusCode.GatewayTimeout => true,
        HttpStatusCode.InsufficientStorage => true,
        _ => false,
    };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        for (var attempt = 1; ; attempt++)
        {
            var last = attempt >= MaxAttempts;

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (last || !IsTransient(response.StatusCode))
                {
                    return response;
                }

                response.Dispose();

                _logger.LogWarning(
                    "Elasticsearch answered {Status} for {Method} {Path}; retrying, attempt {Next} of {Max}.",
                    (int)response.StatusCode,
                    request.Method.Method,
                    request.RequestUri?.AbsolutePath,
                    attempt + 1,
                    MaxAttempts);
            }
            catch (HttpRequestException exception) when (!last && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    exception,
                    "Elasticsearch could not be reached for {Method} {Path}; retrying, attempt {Next} of {Max}.",
                    request.Method.Method,
                    request.RequestUri?.AbsolutePath,
                    attempt + 1,
                    MaxAttempts);
            }

            await Task.Delay(Backoff(attempt), _time, cancellationToken);
        }
    }

    private TimeSpan Backoff(int attempt) => _baseDelay <= TimeSpan.Zero
        ? TimeSpan.Zero
        : (_baseDelay * Math.Pow(2, attempt - 1)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
}
