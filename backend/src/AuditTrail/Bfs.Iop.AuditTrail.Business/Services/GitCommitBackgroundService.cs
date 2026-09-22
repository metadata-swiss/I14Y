using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitCommitBackgroundService : BackgroundService
{
    private readonly IMessageQueue<CommitRequest> _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GitCommitBackgroundService> _logger;

    public GitCommitBackgroundService(
        IMessageQueue<CommitRequest> queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<GitCommitBackgroundService> logger)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var commit in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var processorService = scope.ServiceProvider.GetRequiredService<IGitCommitProcessorService>();

                var response = await processorService.ProcessCommitAsync(commit, stoppingToken);

                if (!response.Success)
                {
                    _logger.LogWarning("Something unexpected happened while processing commit: {Message}", response.StdErr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing commit: {Message}", ex.Message);
            }
        }
    }
}
