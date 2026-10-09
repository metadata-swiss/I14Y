using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public sealed class ReindexOrchestrator
{
    private const int BatchSize = 1000;

    private static readonly TimeSpan ReplayRetryDelay = TimeSpan.FromMilliseconds(500);

    private readonly IServiceScopeFactory _scopes;
    private readonly ReindexGate _gate;
    private readonly PendingIndexWrites _pending;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ReindexOrchestrator> _logger;

    public ReindexOrchestrator(
        IServiceScopeFactory scopes,
        ReindexGate gate,
        PendingIndexWrites pending,
        IHostApplicationLifetime lifetime,
        ILogger<ReindexOrchestrator> logger)
    {
        _scopes = scopes;
        _gate = gate;
        _pending = pending;
        _lifetime = lifetime;
        _logger = logger;
    }

    /// <summary>
    ///     Re-applies the single-document writes that arrived while this pass was running.
    /// </summary>
    private async Task ReplayPendingWritesAsync(CancellationToken cancellationToken)
    {
        var pending = _pending.Drain();

        if (pending.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Replaying {Count} changes that arrived during the pass.", pending.Count);

        using var scope = _scopes.CreateScope();

        var writer = scope.ServiceProvider.GetRequiredService<IIncrementalIndexWriter>();

        var failed = await ApplyAsync(pending, writer, cancellationToken);

        if (failed.Count > 0)
        {
            _logger.LogWarning(
                "{Count} of {Total} changes did not replay; trying them once more.",
                failed.Count,
                pending.Count);

            await Task.Delay(ReplayRetryDelay, cancellationToken);

            failed = await ApplyAsync(failed, writer, cancellationToken);
        }

        if (failed.Count > 0)
        {
            _logger.LogError(
                "{Count} changes could not be replayed after the swap and are not queued anywhere. "
                + "Searches answer from stale data for them until the next rebuild. First: {Sample}.",
                failed.Count,
                string.Join(", ", failed.Take(5).Select(x => $"{x.Operation} {x.Target} '{x.Id}'")));
        }
    }

    private async Task<IReadOnlyList<PendingIndexWrite>> ApplyAsync(
        IReadOnlyCollection<PendingIndexWrite> writes,
        IIncrementalIndexWriter writer,
        CancellationToken cancellationToken)
    {
        var failed = new List<PendingIndexWrite>();

        foreach (var write in writes)
        {
            try
            {
                var work = (write.Target, write.Operation) switch
                {
                    (PendingIndexTarget.CatalogResource, PendingIndexOperation.Upsert) =>
                        writer.UpsertCatalogResourceAsync(
                            write.ResourceType ?? throw new InvalidOperationException(
                                $"The pending upsert of '{write.Id}' carries no resource type."),
                            write.Id,
                            cancellationToken),
                    (PendingIndexTarget.CatalogResource, PendingIndexOperation.Remove) =>
                        writer.RemoveCatalogResourceAsync(write.Id, cancellationToken),
                    (PendingIndexTarget.CodeList, PendingIndexOperation.Upsert) =>
                        writer.ReplaceCodeListAsync(write.Id, cancellationToken),
                    (PendingIndexTarget.CodeList, PendingIndexOperation.Remove) =>
                        writer.RemoveCodeListAsync(write.Id, cancellationToken),
                    _ => throw new NotSupportedException(
                        $"'{write.Target}' with '{write.Operation}' cannot be replayed."),
                };

                await work;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    exception,
                    "Could not replay the {Operation} of {Target} '{Id}' after the swap.",
                    write.Operation,
                    write.Target,
                    write.Id);

                failed.Add(write);
            }
        }

        return failed;
    }

    private void DiscardJournal(string operation)
    {
        var discarded = _pending.Drain();

        if (discarded.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Discarded {Count} journalled writes from the failed {Operation}. They already reached the "
            + "live indices, which this pass never replaced.",
            discarded.Count,
            operation);
    }

    public bool TryStart()
    {
        const string operation = "reindex";

        if (!_gate.TryBegin(operation))
        {
            _logger.LogInformation("Refused a {Operation} because one is already running.", operation);

            return false;
        }

        _ = Task.Run(() => RunAsync(operation));

        return true;
    }

    private async Task RunAsync(string operation)
    {
        var succeeded = false;

        IndexRebuildReport? catalog = null;
        IndexRebuildReport? codeLists = null;

        try
        {
            var cancellationToken = _lifetime.ApplicationStopping;

            using var scope = _scopes.CreateScope();
            var provider = scope.ServiceProvider;

            var provisioner = provider.GetRequiredService<ElasticsearchIndexProvisioner>();

            var prepared = await provisioner.PrepareAsync(cancellationToken);

            provider.GetRequiredService<IndexWriteTarget>()
                .RedirectTo(prepared.Catalog, prepared.CodeList);

            _logger.LogInformation(
                "Building {Catalog} and {CodeList} aside; the live indices keep answering.",
                prepared.Catalog,
                prepared.CodeList);

            try
            {
                catalog = await provider.GetRequiredService<CatalogIndexRebuilder>()
                    .RebuildAsync(BatchSize, cancellationToken);

                codeLists = await provider.GetRequiredService<CodeListIndexRebuilder>()
                    .RebuildAsync(BatchSize, cancellationToken);

                EnsureComplete(catalog, "catalog");
                EnsureComplete(codeLists, "code list");

                await EnsureStructuresNotLostAsync(provisioner, catalog, cancellationToken);
            }
            catch
            {
                await provisioner.DiscardAsync(prepared, CancellationToken.None);
                throw;
            }

            await provisioner.PublishAsync(prepared, cancellationToken);

            // The aliases have moved, so the pass has done what it was asked to do. Anything after
            // this is housekeeping and must not be able to report the swap as not having happened.
            succeeded = true;

            await ReplayPendingWritesAsync(CancellationToken.None);

            try
            {
                await provisioner.ForceMergeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                // Starting the merge can still fail on the transport or on shutdown, where the
                // provisioner's own handling never runs. The documents are published and searchable
                // either way; they simply sit in more segments than they need to.
                _logger.LogWarning(
                    exception,
                    "Could not start the force merge after {Operation}. The indices are published and "
                    + "searchable; they keep the segments the pass left.",
                    operation);
            }

            _logger.LogInformation(
                "{Operation} finished. Catalog {CatalogWritten}/{CatalogSent}, code lists "
                + "{CodeListWritten}/{CodeListSent}, {Failed} batches failed.",
                operation,
                catalog.DocumentsWritten,
                catalog.DocumentsSent,
                codeLists.DocumentsWritten,
                codeLists.DocumentsSent,
                catalog.BatchesFailed + codeLists.BatchesFailed);
        }
        catch (Exception exception)
        {
            // Nothing may escape: this runs detached, so an exception thrown here would be lost rather
            // than reported. Which message is true depends on whether the aliases had already moved.
            if (succeeded)
            {
                _logger.LogError(
                    exception,
                    "A {Operation} published its indices and then failed while finishing up. The new "
                    + "indices are live.",
                    operation);
            }
            else
            {
                _logger.LogError(exception, "A {Operation} failed. The live indices are unchanged.", operation);

                DiscardJournal(operation);
            }
        }
        finally
        {
            _gate.End(succeeded, catalog, codeLists);
        }
    }

    private static async Task EnsureStructuresNotLostAsync(
        ElasticsearchIndexProvisioner provisioner,
        IndexRebuildReport catalog,
        CancellationToken cancellationToken)
    {
        if (catalog.StructuresResolved == true)
        {
            return;
        }

        if (!await provisioner.CatalogHasStructureFlagsAsync(cancellationToken))
        {
            return;
        }

        throw new InvalidOperationException(
            "The dataset structures could not be read, but the catalog currently in use carries them. "
            + "Publishing would have replaced a populated Structures facet with an empty one and "
            + "deleted the index holding it, so the prepared indices were discarded. Check the triple "
            + "store configuration.");
    }

    private static void EnsureComplete(IndexRebuildReport report, string what)
    {
        if (report.BatchesFailed == 0 && report.DocumentsWritten == report.DocumentsSent)
        {
            return;
        }

        throw new InvalidOperationException(
            $"The {what} pass wrote {report.DocumentsWritten} of {report.DocumentsSent} documents and "
            + $"lost {report.BatchesFailed} batches, so the prepared indices were discarded rather "
            + "than published.");
    }
}
