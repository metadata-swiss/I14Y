using Bfs.Iop.Common.Messaging;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Messaging.SearchIndex;

/// <summary>
///     Carries queued changes to the search service, off the request thread.
/// </summary>
internal sealed class SearchIndexDispatcherService : BackgroundService
{
    private const int MaxRetriesInCaseOfFail = 10;

    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan LongestRetryDelay = TimeSpan.FromMinutes(1);

    private readonly IMessageQueue<SearchIndexMessage> _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SearchIndexDispatcherService> _logger;

    public SearchIndexDispatcherService(
        IMessageQueue<SearchIndexMessage> queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SearchIndexDispatcherService> logger)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await DeliverAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task DeliverAsync(SearchIndexMessage message, CancellationToken stoppingToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<IIndexSearchApiClient>();

                await SendAsync(client, message, stoppingToken);

                return;
            }
            catch (ApiException rejected)
                when (IsPermanent(rejected.StatusCode) && !stoppingToken.IsCancellationRequested)
            {     
                _logger.LogError(
                    rejected,
                    "The search service rejected the {Operation} of {Target} '{Id}' with "
                    + "{StatusCode}. Retrying cannot change that, so it was dropped. Searches will "
                    + "answer from stale data for this resource until the next rebuild.",
                    message.Operation,
                    message.Target,
                    message.Id,
                    rejected.StatusCode);

                return;
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                if (attempt >= MaxRetriesInCaseOfFail)
                {
                    _logger.LogError(
                        ex,
                        "Gave up telling the search service about {Operation} of {Target} '{Id}' after "
                        + "{Attempts} attempts. Searches will answer from stale data for this resource "
                        + "until the next rebuild.",
                        message.Operation,
                        message.Target,
                        message.Id,
                        MaxRetriesInCaseOfFail);

                    return;
                }

                _logger.LogWarning(
                    ex,
                    "Could not tell the search service about {Operation} of {Target} '{Id}'. "
                    + "Attempt {Attempt} of {Attempts}.",
                    message.Operation,
                    message.Target,
                    message.Id,
                    attempt,
                    MaxRetriesInCaseOfFail);

                await Task.Delay(RetryDelay(attempt), stoppingToken);
            }
        }
    }

    /// <summary>
    ///     Whether the search service's answer says the request itself is wrong, rather than that it
    ///     could not be served right now.
    /// </summary>
    /// <remarks>
    ///     A timeout and a rate limit arrive as 4xx but describe the moment, not the message, so both
    ///     are still worth retrying.
    /// </remarks>
    private static bool IsPermanent(int statusCode) =>
        statusCode is >= 400 and < 500
        && statusCode is not (StatusCodes.Status408RequestTimeout or StatusCodes.Status429TooManyRequests);

    private static TimeSpan RetryDelay(int attempt)
    {
        var doublings = Math.Min(attempt - 1, 20);
        var delay = FirstRetryDelay * Math.Pow(2, doublings);

        return delay < LongestRetryDelay ? delay : LongestRetryDelay;
    }

    private static Task SendAsync(
        IIndexSearchApiClient client,
        SearchIndexMessage message,
        CancellationToken cancellationToken) => (message.Target, message.Operation) switch
        {
            (SearchIndexTarget.CatalogResource, SearchIndexOperation.Upsert) =>
                client.PutIndexCatalogByTypeAndIdAsync(
                    message.ResourceType ?? throw new InvalidOperationException(
                        $"The change of the catalogue resource '{message.Id}' carries no resource type."),
                    message.Id,
                    cancellationToken),
            (SearchIndexTarget.CatalogResource, SearchIndexOperation.Remove) =>
                client.DeleteIndexCatalogByIdAsync(message.Id, cancellationToken),
            (SearchIndexTarget.CodeList, SearchIndexOperation.Upsert) =>
                client.PutIndexCodelistByConceptIdAsync(message.Id, cancellationToken),
            (SearchIndexTarget.CodeList, SearchIndexOperation.Remove) =>
                client.DeleteIndexCodelistByConceptIdAsync(message.Id, cancellationToken),
            _ => throw new NotSupportedException(
                $"'{message.Target}' with '{message.Operation}' cannot be sent."),
        };
}
