using Bfs.Iop.Search.Abstractions;
using Bfs.Iop.Search.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>
/// Owns the lifecycle of both Elasticsearch indexes. This is the <b>only</b> place in the solution
/// that creates or rebuilds one — IOP Core holds no index and does nothing indexing-related at
/// startup.
/// <para>
/// On startup it drops and recreates both indexes, then builds them from Postgres
/// (<c>Elasticsearch:RecreateIndexOnStartup</c>, true by default — see that setting for why a fresh
/// index rather than a build over the existing one, and for the empty-results window that buys).
/// After that it rebuilds on a schedule or on request.
/// </para>
/// <para>
/// Note the asymmetry, because it is easy to misread: only <b>startup</b> recreates. The scheduled
/// rebuild and <c>POST /api/Index/rebuild</c> both go straight to <see cref="BuildBothIndexesAsync"/>,
/// which is upsert-only — it will not purge a document whose row has been deleted from Postgres.
/// Between restarts, a lost de-index event is still permanent.
/// </para>
/// <para>
/// The periodic rebuild is the safety net that makes the fire-and-forget trigger design acceptable:
/// a trigger that never arrived, a batch that failed, or a queue lost on restart all heal within one
/// interval. Removing it turns every logged failure into permanent index drift.
/// </para>
/// <para>
/// The index services are scoped (they consume the scoped user context), so every use here goes
/// through <see cref="IServiceScopeFactory"/> rather than constructor injection — a hosted service
/// is a singleton and cannot hold them.
/// </para>
/// </summary>
internal sealed class IndexBuilderHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIndexBuildState _buildState;
    private readonly IIndexRebuildTrigger _rebuildTrigger;
    private readonly IndexSearchOptions _options;
    private readonly ElasticsearchOptions _elasticOptions;
    private readonly ILogger<IndexBuilderHostedService> _logger;

    public IndexBuilderHostedService(
        IServiceScopeFactory scopeFactory,
        IIndexBuildState buildState,
        IIndexRebuildTrigger rebuildTrigger,
        IOptions<IndexSearchOptions> options,
        IOptions<ElasticsearchOptions> elasticOptions,
        ILogger<IndexBuilderHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _buildState = buildState;
        _rebuildTrigger = rebuildTrigger;
        _options = options.Value;
        _elasticOptions = elasticOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create if missing, never recreate. Recreation belongs to BuildBothIndexesAsync, so that
        // dropping an index and refilling it are one operation that cannot be half-performed.
        try
        {
            using var scope = _scopeFactory.CreateScope();

            await scope.ServiceProvider.GetRequiredService<ICatalogIndexService>()
                .EnsureIndexAsync(recreate: false, stoppingToken);
            await scope.ServiceProvider.GetRequiredService<ICodeListEntryIndexService>()
                .EnsureIndexAsync(recreate: false, stoppingToken);
        }
        catch (Exception ex)
        {
            // Without an index nothing else can work, but crashing the host would just have the
            // platform restart us into the same failure; surface it and let /health report unready.
            _logger.LogError(ex, "Failed to ensure the Elasticsearch indexes exist.");
            return;
        }

        // Evaluated even when no build follows, because that combination is the misconfiguration
        // ShouldRecreate exists to report.
        var recreateAtStartup = ShouldRecreate();

        if (_options.BuildIndexOnStartup)
        {
            await SafeFullBuildAsync(recreateAtStartup, stoppingToken);
        }

        // Wake on whichever comes first: the schedule, or an explicit POST /api/Index/rebuild.
        var interval = _options.FullReindexIntervalHours > 0
            ? TimeSpan.FromHours(_options.FullReindexIntervalHours)
            : Timeout.InfiniteTimeSpan;

        if (interval == Timeout.InfiniteTimeSpan)
        {
            _logger.LogWarning(
                "Periodic full reindex is disabled (IndexSearch:FullReindexIntervalHours = {Hours}). " +
                "Index drift from failed or lost triggers will NOT self-heal on its own; only an " +
                "explicit rebuild request will repair it.",
                _options.FullReindexIntervalHours);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var wakeUp = await _rebuildTrigger.WaitForRequestAsync(interval, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            // A scheduled run recreates only when configured to; a requested one recreates only when
            // the caller asked. POST /api/Index/rebuild therefore stays non-destructive, and
            // POST /api/Index/recreate is the one that purges.
            var recreate = wakeUp.Recreate
                || (!wakeUp.Requested && _options.RecreateOnScheduledRebuild);

            if (wakeUp.Requested)
            {
                _logger.LogInformation(
                    "Full index build requested through the API. Recreating first: {Recreate}.",
                    recreate);
            }

            await SafeFullBuildAsync(recreate, stoppingToken);
        }
    }

    /// <summary>
    /// Whether to drop both indexes before ensuring them.
    /// <para>
    /// Recreating is only ever safe when a build follows it in the same pass. <c>EnsureIndexAsync</c>
    /// deletes and recreates an <i>empty</i> index; if <c>BuildIndexOnStartup</c> is off, nothing
    /// refills it and the service serves zero results indefinitely — not until the next restart, but
    /// until someone notices and calls <c>POST /api/Index/rebuild</c> by hand. The scheduled rebuild
    /// would eventually repopulate it, up to <c>FullReindexIntervalHours</c> later.
    /// </para>
    /// <para>
    /// So the two settings are not independent, and the safe direction when they conflict is to keep
    /// a stale-but-populated index rather than create a fresh empty one. Logged as an error because
    /// it is a misconfiguration, not a mode.
    /// </para>
    /// </summary>
    internal static bool ShouldRecreate(bool recreateOnStartup, bool buildOnStartup, ILogger logger)
    {
        if (!recreateOnStartup)
        {
            return false;
        }

        if (buildOnStartup)
        {
            return true;
        }

        logger.LogError(
            "Elasticsearch:RecreateIndexOnStartup is true but IndexSearch:BuildIndexOnStartup is " +
            "false. Dropping the indexes without rebuilding them would leave every search returning " +
            "no results until a rebuild is requested. Keeping the existing indexes instead — set " +
            "BuildIndexOnStartup to true if a fresh index at startup was intended.");

        return false;
    }

    private bool ShouldRecreate() =>
        ShouldRecreate(_elasticOptions.RecreateIndexOnStartup, _options.BuildIndexOnStartup, _logger);

    private async Task SafeFullBuildAsync(bool recreate, CancellationToken cancellationToken)
    {
        try
        {
            await _buildState.RunFullBuildAsync(ct => BuildBothIndexesAsync(recreate, ct), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception ex) when (recreate)
        {
            // Distinct from the message below, because the state is not the same. The indexes were
            // already dropped, so what is left is whatever the build managed to write before it
            // failed — nothing at all, or a partial index, depending on how far it got. Either way
            // searches now return too few results rather than stale ones, and if
            // FullReindexIntervalHours is 0 there is no scheduled run to recover it.
            //
            // Deliberately does not claim the indexes are empty: the catalog build runs to completion
            // before the code-list one starts, so a failure in the second leaves the first fully
            // populated. An operator told "empty" who then sees results stops trusting the log.
            _logger.LogError(
                ex,
                "Full index build failed AFTER the indexes were dropped, so they are now empty or " +
                "only partly rebuilt and searches will return too few results. Check GET /api/Index " +
                "— 'ready' stays false until a build completes. Fix the cause, most often the " +
                "database being unreachable, then POST /api/Index/recreate to retry.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full index build failed. The next scheduled run will retry.");
        }
    }

    internal async Task<bool?> BuildBothIndexesAsync(bool recreate, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting full index build. Recreating first: {Recreate}.", recreate);

        using var scope = _scopeFactory.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IIndexBuilderService>();
        var codeListIndex = scope.ServiceProvider.GetRequiredService<ICodeListEntryIndexService>();

        if (recreate)
        {
            // Readiness has to drop with the documents, and it has to drop BEFORE they go, because
            // the build may never finish. IsReady latches on the first success and no failure clears
            // it, so without this a recreate that dies mid-build would keep reporting ready:true over
            // an empty index — and the readiness probe would keep the replica in rotation.
            _buildState.MarkNotReady();

            // Immediately before the build, never earlier: from here until the build completes the
            // indexes are empty and searches return 200 with no results.
            await scope.ServiceProvider.GetRequiredService<ICatalogIndexService>()
                .EnsureIndexAsync(recreate: true, cancellationToken);
            await codeListIndex.EnsureIndexAsync(recreate: true, cancellationToken);
        }

        var report = await builder.BuildIndexAsync(cancellationToken);
        await codeListIndex.BuildIndexAsync(cancellationToken);

        _logger.LogInformation(
            "Full index build completed. Dataset structures available: {StructuresAvailable}.",
            report.StructuresAvailable);

        return report.StructuresAvailable;
    }
}
