using Bfs.Iop.Common.Messaging;
using Bfs.Iop.IndexSearch.ApiClient;
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
    private const int TimeBetweenRetriesinMs = 300;

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
                using var scope = _serviceScopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<IIndexSearchApiClient>();

                await SendAsync(client, message, stoppingToken);
            }
            catch (Exception ex)
            {
                if (message.FailCount < MaxRetriesInCaseOfFail)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not tell the search service about {Operation} of {Target} '{Id}'. "
                        + "Retry {RetryCount}.",
                        message.Operation,
                        message.Target,
                        message.Id,
                        message.FailCount);

                    await Task.Delay(TimeBetweenRetriesinMs, stoppingToken);

                    await _queue.EnqueueAsync(message with { FailCount = message.FailCount + 1 }, stoppingToken);

                    continue;
                }

                _logger.LogError(
                    ex,
                    "Gave up telling the search service about {Operation} of {Target} '{Id}' after "
                    + "{Attempts} attempts. Searches will answer from stale data for this resource "
                    + "until the next rebuild.",
                    message.Operation,
                    message.Target,
                    message.Id,
                    MaxRetriesInCaseOfFail);
            }
        }
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
