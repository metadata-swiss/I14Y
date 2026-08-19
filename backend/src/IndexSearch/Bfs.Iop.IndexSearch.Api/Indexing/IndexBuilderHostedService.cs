using Bfs.Iop.Search.Abstractions;
using Bfs.Iop.Search.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Ensures both indexes exist at startup, optionally builds them, and then rebuilds them on a
/// schedule.
/// <para>
/// The periodic rebuild is the safety net that makes the fire-and-forget trigger design acceptable:
/// a trigger that never arrived, a batch that failed, or a queue lost on restart all heal within one
/// interval. Removing it turns every logged failure into permanent index drift.
/// </para>
/// </summary>
internal sealed class IndexBuilderHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICatalogIndexService _catalogIndex;
    private readonly ICodeListEntryIndexService _codeListIndex;
    private readonly IIndexBuildState _buildState;
    private readonly IndexSearchOptions _options;
    private readonly ElasticsearchOptions _elasticOptions;
    private readonly ILogger<IndexBuilderHostedService> _logger;

    public IndexBuilderHostedService(
        IServiceScopeFactory scopeFactory,
        ICatalogIndexService catalogIndex,
        ICodeListEntryIndexService codeListIndex,
        IIndexBuildState buildState,
        IOptions<IndexSearchOptions> options,
        IOptions<ElasticsearchOptions> elasticOptions,
        ILogger<IndexBuilderHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _catalogIndex = catalogIndex;
        _codeListIndex = codeListIndex;
        _buildState = buildState;
        _options = options.Value;
        _elasticOptions = elasticOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _catalogIndex.EnsureIndexAsync(_elasticOptions.RecreateIndexOnStartup, stoppingToken);
            await _codeListIndex.EnsureIndexAsync(_elasticOptions.RecreateIndexOnStartup, stoppingToken);
        }
        catch (Exception ex)
        {
            // Without an index nothing else can work, but crashing the host would just have the
            // platform restart us into the same failure; surface it and let /health report unready.
            _logger.LogError(ex, "Failed to ensure the Elasticsearch indexes exist.");
            return;
        }

        if (_options.BuildIndexOnStartup)
        {
            await SafeFullBuildAsync(stoppingToken);
        }

        if (_options.FullReindexIntervalHours <= 0)
        {
            _logger.LogWarning(
                "Periodic full reindex is disabled (IndexSearch:FullReindexIntervalHours = {Hours}). " +
                "Index drift from failed or lost triggers will NOT self-heal.",
                _options.FullReindexIntervalHours);
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.FullReindexIntervalHours));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SafeFullBuildAsync(stoppingToken);
        }
    }

    private async Task SafeFullBuildAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _buildState.RunFullBuildAsync(BuildBothIndexesAsync, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full index build failed. The next scheduled run will retry.");
        }
    }

    private async Task BuildBothIndexesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting full index build.");

        using var scope = _scopeFactory.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IIndexBuilderService>();

        await builder.BuildIndexAsync(cancellationToken);
        await _codeListIndex.BuildIndexAsync(cancellationToken);

        _logger.LogInformation("Full index build completed.");
    }
}
