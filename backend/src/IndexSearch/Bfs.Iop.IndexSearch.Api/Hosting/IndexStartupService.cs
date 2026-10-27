using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class IndexStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ReindexOrchestrator _orchestrator;
    private readonly ILogger<IndexStartupService> _logger;

    public IndexStartupService(
        IServiceScopeFactory scopes,
        ReindexOrchestrator orchestrator,
        ILogger<IndexStartupService> logger)
    {
        _scopes = scopes;
        _orchestrator = orchestrator;
        _logger = logger;
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

            await FillIfEmptyAsync(provisioner, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "The index startup work failed. Search runs against the existing index.");
        }
    }

    private async Task FillIfEmptyAsync(
        ElasticsearchIndexProvisioner provisioner,
        CancellationToken cancellationToken)
    {
        if (!await provisioner.CatalogIsEmptyAsync(cancellationToken))
        {
            return;
        }

        _logger.LogInformation(
            "The catalogue is empty, so a first pass was started rather than waiting for the next "
            + "scheduled slot.");

        if (!_orchestrator.TryStart())
        {
            _logger.LogInformation("A pass was already running, so the empty catalogue is already being filled.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
