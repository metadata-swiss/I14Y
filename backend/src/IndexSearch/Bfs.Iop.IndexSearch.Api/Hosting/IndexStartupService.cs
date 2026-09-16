using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

internal sealed class IndexStartupService : IHostedService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<IndexStartupService> _logger;

    public IndexStartupService(IServiceScopeFactory scopes, ILogger<IndexStartupService> logger)
    {
        _scopes = scopes;
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
            using (var scope = _scopes.CreateScope())
            {
                var provisioner = scope.ServiceProvider
                    .GetRequiredService<ElasticsearchIndexProvisioner>();

                await provisioner.CreateIfMissingAsync(CancellationToken.None);
                await provisioner.SweepOrphansAsync(CancellationToken.None);
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
