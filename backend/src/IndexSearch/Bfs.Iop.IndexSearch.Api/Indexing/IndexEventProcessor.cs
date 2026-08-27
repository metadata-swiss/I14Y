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

    /// <summary>
    /// Collapses repeats: several writes to the same resource in quick succession need only one
    /// reconcile, because a reconcile applies the payload it is given rather than a delta.
    /// <para>
    /// Keyed on <c>(Target, Id)</c> and deliberately NOT on the whole event.
    /// <see cref="IndexEvent"/> is a record whose <c>Payload</c> is <c>object?</c>, so record
    /// equality compares payloads by <b>reference</b>: two forwards of the same dataset carry two
    /// different payload instances, and <c>Distinct()</c> keeps both. It would collapse only
    /// duplicate deletes, where <c>Payload</c> is null — the opposite of what this is for, and
    /// exactly the bug this method replaced.
    /// </para>
    /// <para>
    /// Last wins, which is correct in both orders: <c>[update, delete]</c> collapses to the delete,
    /// <c>[delete, update]</c> to the update. The queue preserves arrival order, so the last entry
    /// for a resource is the most recent intent for it.
    /// </para>
    /// <para>
    /// Extracted from the loop purely so it can be tested without starting a
    /// <see cref="BackgroundService"/> — the behaviour is a pure function of the batch.
    /// </para>
    /// </summary>
    internal static IReadOnlyCollection<IndexEvent> Coalesce(IReadOnlyCollection<IndexEvent> batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return [.. batch.GroupBy(x => (x.Target, x.Id)).Select(g => g.Last())];
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

            var deduplicated = Coalesce(batch);

            try
            {
                // Gated for the whole write, not merely before it. Releasing the gate first and then
                // writing leaves a window in which the builder can drop the index underneath this
                // batch, and the bulk request would then auto-create it with a dynamic mapping.
                await _buildState.RunGatedAsync(async ct =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reconciler = scope.ServiceProvider.GetRequiredService<IIndexReconciler>();
                    await reconciler.ReconcileAsync(deduplicated, ct);
                }, stoppingToken);

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
