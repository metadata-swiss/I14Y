using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class IndexStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ReindexOrchestrator _orchestrator;
    private readonly IndexSearchOptions _options;
    private readonly ILogger<IndexStartupService> _logger;

    public IndexStartupService(IServiceScopeFactory scopes, ILogger<IndexStartupService> logger)
    {
        _scopes = scopes;
        _orchestrator = orchestrator;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.OnStartup == StartupAction.None)
        {
            return Task.CompletedTask;
        }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopes.CreateScope();

            var provisioner = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexProvisioner>();

            await provisioner.CreateIfMissingAsync(cancellationToken);
            await provisioner.SweepOrphansAsync(cancellationToken);

            _logger.LogInformation(
                "The indices exist and any abandoned generations have been reclaimed.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "The index startup work failed. Search runs against the existing index.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
