using Microsoft.Extensions.Hosting;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitCommitBackgroundService : BackgroundService
{
    private readonly GitCommitProcessorService _gitCommitService;

    public GitCommitBackgroundService(GitCommitProcessorService gitCommitService) =>
        _gitCommitService = gitCommitService;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        _gitCommitService.ProcessQueueAsync(stoppingToken);
}
