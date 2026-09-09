using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class IndexStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ReindexOrchestrator _orchestrator;
    private readonly IndexSearchOptions _options;
    private readonly ILogger<IndexStartupService> _logger;

    public IndexStartupService(
        IServiceScopeFactory scopes,
        ReindexOrchestrator orchestrator,
        IOptions<IndexSearchOptions> options,
        ILogger<IndexStartupService> logger)
    {
        _scopes = scopes;
        _orchestrator = orchestrator;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.CreateIndicesOnStartup && !_options.ReindexOnStartup)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(RunAsync, CancellationToken.None);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task RunAsync()
    {
        try
        {
            if (_options.CreateIndicesOnStartup)
            {
                using var scope = _scopes.CreateScope();

                var provisioner = scope.ServiceProvider
                    .GetRequiredService<ElasticsearchIndexProvisioner>();

                await provisioner.CreateIfMissingAsync(CancellationToken.None);
                await provisioner.SweepOrphansAsync(CancellationToken.None);
            }

            if (!_options.ReindexOnStartup)
            {
                return;
            }

            if (!_orchestrator.TryStart(_options.ResetOnStartup))
            {
                _logger.LogWarning("Skipped the startup reindex because one was already running.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "The index startup work failed. Search runs against the existing index.");
        }
    }
}
