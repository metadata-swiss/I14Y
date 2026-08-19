using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Single-consumer worker that drains <see cref="IIndexEventQueue"/> and reconciles each batch.
/// <para>
/// Deliberately one consumer: the reconciler resolves scoped data services backed by a single
/// <c>IopDbContext</c>, which cannot be used concurrently — the same constraint the full index
/// builder documents.
/// </para>
/// </summary>
internal sealed class IndexEventProcessor : BackgroundService
{
    private readonly IIndexEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIndexBuildState _buildState;
    private readonly IndexSearchOptions _options;
    private readonly ILogger<IndexEventProcessor> _logger;

    public IndexEventProcessor(
        IIndexEventQueue queue,
        IServiceScopeFactory scopeFactory,
        IIndexBuildState buildState,
        IOptions<IndexSearchOptions> options,
        ILogger<IndexEventProcessor> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _buildState = buildState;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IReadOnlyCollection<IndexEvent> batch;

            try
            {
                batch = await _queue.DequeueBatchAsync(_options.BatchSize, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // Collapse repeats: several writes to the same resource in quick succession only need one
            // reconcile, because a reconcile reads the current state rather than applying a delta.
            var deduplicated = batch.Distinct().ToList();

            // Don't interleave single-document writes with a full rebuild of the same index.
            await _buildState.WaitWhileFullBuildRunningAsync(stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reconciler = scope.ServiceProvider.GetRequiredService<IIndexReconciler>();
                await reconciler.ReconcileAsync(deduplicated, stoppingToken);

                _logger.LogDebug("Reconciled {Count} index event(s) ({Raw} received).", deduplicated.Count, batch.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // Never let a failure end the loop: the service would silently stop indexing.
                _logger.LogError(ex, "Index event batch failed; {Count} event(s) dropped. The periodic full reindex will repair the drift.", deduplicated.Count);
            }
        }
    }
}
