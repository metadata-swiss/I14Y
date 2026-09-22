using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed class AuditTrailDispatcherService : BackgroundService
{
    private const int MaxRetriesInCaseOfFail = 10;

    private readonly IMessageQueue<AuditTrailMessage> _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuditTrailDispatcherService> _logger;

    public AuditTrailDispatcherService(
        IMessageQueue<AuditTrailMessage> queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditTrailDispatcherService> logger)
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
                var client = scope.ServiceProvider.GetRequiredService<IAuditTrailApiClient>();

                await client.CommitAsync(message.CommitRequest, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Message dispatching failed. Retry {RetryCount}.", message.FailCount);

                if (message.FailCount < MaxRetriesInCaseOfFail)
                {
                    var retryMessage = message with
                    {
                        FailCount = message.FailCount + 1
                    };

                    await _queue.EnqueueAsync(retryMessage, stoppingToken);
                }
            }
        }
    }
}
