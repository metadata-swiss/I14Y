using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.IndexSearch.ApiClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.IndexForwarding;

/// <summary>
/// Drains the forward queue and posts each group of resources to the IndexSearch service.
/// <para>
/// Runs in IOP Core's process, off the request path: a save returns as soon as the item is queued,
/// and any HTTP slowness or outage is absorbed here instead of in the user's request.
/// </para>
/// </summary>
internal sealed class IndexForwardingSenderHostedService : BackgroundService
{
    /// <summary>
    /// Maximum resources sent in one HTTP request. A constant for the same reason as
    /// <c>IndexSearchDispatcher.QueueCapacity</c>: the configuration section that used to bind it was
    /// set nowhere, so this was always the effective value.
    /// </summary>
    private const int BatchSize = 100;

    /// <summary>
    /// Which client call each queued target maps to. A table rather than a <c>switch</c> so the
    /// target-to-endpoint mapping is one readable block; the type argument stays explicit per target
    /// because that is what decides how the payload serialises.
    /// </summary>
    private static readonly Dictionary<IndexForwardTarget,
        Func<IIndexSearchApiClient, List<IndexForwardItem>, CancellationToken, Task>> _senders = new()
    {
        [IndexForwardTarget.Catalog] = (c, i, ct) => c.IndexCatalogAsync(Models<CatalogIndexEntry>(i), ct),
        [IndexForwardTarget.CodeListEntries] = (c, i, ct) => c.IndexCodeListEntriesAsync(Models<CodeListIndexEntry>(i), ct),
        [IndexForwardTarget.CatalogDelete] = (c, i, ct) => c.DeIndexCatalogAsync(Ids(i), ct),
        [IndexForwardTarget.CodeListEntryDelete] = (c, i, ct) => c.DeIndexCodeListEntriesAsync(Ids(i), ct),
    };

    private readonly IIndexSearchDispatcher _dispatcher;
    private readonly IIndexSearchApiClient _client;
    private readonly ILogger<IndexForwardingSenderHostedService> _logger;

    public IndexForwardingSenderHostedService(
        IIndexSearchDispatcher dispatcher,
        IIndexSearchApiClient client,
        ILogger<IndexForwardingSenderHostedService> logger)
    {
        _dispatcher = dispatcher;
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IReadOnlyCollection<IndexForwardItem> batch;

            try
            {
                batch = await _dispatcher.DequeueBatchAsync(BatchSize, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            foreach (var group in batch.GroupBy(x => x.Target))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                await SendGroupAsync(group.Key, [.. group], stoppingToken);
            }
        }
    }

    private async Task SendGroupAsync(IndexForwardTarget target, List<IndexForwardItem> items, CancellationToken cancellationToken)
    {
        // Looks unreachable — the table covers every declared target — and that is exactly why it
        // stays. It is the guard for a target added to the enum but not to the table, which would
        // otherwise fail in complete silence: the enqueue succeeds, nothing is sent, and the
        // documents are simply missing from search until the next full reindex.
        if (!_senders.TryGetValue(target, out var send))
        {
            _logger.LogWarning("Ignoring {Count} forward item(s) with unsupported target {Target}.", items.Count, target);

            return;
        }

        try
        {
            await send(_client, items, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutting down; the periodic full reindex will pick up whatever did not make it.
        }
        catch (Exception ex)
        {
            // Deliberately not retried here: the data is already committed to Postgres, so the index
            // is merely stale, and the IndexSearch service rebuilds from Postgres on a schedule.
            // Retrying in this loop would delay newer, more relevant work behind a failing batch.
            _logger.LogError(
                ex,
                "Failed to forward {Count} {Target} item(s) to the IndexSearch service. " +
                "The index is stale for those resources until the next full reindex.",
                items.Count,
                target);
        }
    }

    private static List<T> Models<T>(List<IndexForwardItem> items) => [.. items.Select(x => x.Model).OfType<T>()];

    private static List<Guid> Ids(List<IndexForwardItem> items) =>
        [.. items.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).Distinct()];
}
