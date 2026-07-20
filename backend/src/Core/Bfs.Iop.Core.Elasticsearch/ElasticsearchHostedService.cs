using Bfs.Iop.Core.Elasticsearch.CodeList;
using Bfs.Iop.Core.Lucene.IndexBuilders;
using Bfs.Iop.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Elasticsearch;

/// <summary>
/// Creates the catalog index (with mapping) if needed and builds it in the background at startup.
/// Elasticsearch counterpart of <c>LuceneHostedService</c>.
/// </summary>
internal sealed class ElasticsearchHostedService : IHostedService
{
    private readonly ElasticsearchCatalogIndexService _indexService;
    private readonly ElasticsearchCodeListEntryIndexService _codeListIndexService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticsearchHostedService> _logger;

    public ElasticsearchHostedService(
        ElasticsearchCatalogIndexService indexService,
        ElasticsearchCodeListEntryIndexService codeListIndexService,
        IServiceScopeFactory scopeFactory,
        IOptions<ElasticsearchOptions> options,
        ILogger<ElasticsearchHostedService> logger)
    {
        _indexService = indexService;
        _codeListIndexService = codeListIndexService;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Elasticsearch indexes build in background...");

        _ = Task.Run(async () =>
        {
            try
            {
                await _indexService.EnsureIndexAsync(_options.RecreateIndexOnStartup, cancellationToken);
                await _codeListIndexService.EnsureIndexAsync(_options.RecreateIndexOnStartup, cancellationToken);

                using var scope = _scopeFactory.CreateScope();
                var builder = scope.ServiceProvider.GetRequiredService<IIndexBuilderService>();
                await builder.BuildIndex(cancellationToken);
                _logger.LogInformation("Elasticsearch catalog index built successfully.");

                await _codeListIndexService.BuildIndex(cancellationToken);
                _logger.LogInformation("Elasticsearch codelist index built successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build Elasticsearch catalog index.");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
