using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public sealed class ReindexOrchestrator
{
    private const int BatchSize = 1000;

    private readonly IServiceScopeFactory _scopes;
    private readonly ReindexGate _gate;
    private readonly IndexSearchOptions _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<ReindexOrchestrator> _logger;

    public ReindexOrchestrator(
        IServiceScopeFactory scopes,
        ReindexGate gate,
        IOptions<IndexSearchOptions> options,
        IHostApplicationLifetime lifetime,
        ILogger<ReindexOrchestrator> logger)
    {
        _scopes = scopes;
        _gate = gate;
        _lifetime = lifetime;
        _logger = logger;
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

            PreparedIndices? prepared = null;

                provider.GetRequiredService<IndexWriteTarget>()
                    .RedirectTo(prepared.Catalog, prepared.CodeList);

                _logger.LogInformation(
                    "Building {Catalog} and {CodeList} aside; the live indices keep answering.",
                    prepared.Catalog,
                    prepared.CodeList);
            }

            try
            {
                catalog = await provider.GetRequiredService<CatalogIndexRebuilder>()
                    .RebuildAsync(BatchSize, cancellationToken);

                codeLists = await provider.GetRequiredService<CodeListIndexRebuilder>()
                    .RebuildAsync(BatchSize, cancellationToken);

                EnsureComplete(catalog, "catalog");
                EnsureComplete(codeLists, "code list");
            }
            catch when (prepared is not null)
            {
                await provisioner.DiscardAsync(prepared, CancellationToken.None);
                throw;
            }

            if (prepared is not null)
            {
                await provisioner.PublishAsync(prepared, cancellationToken);

            // The aliases have moved, so the pass has done what it was asked to do. Anything after
            // this is housekeeping and must not be able to report the swap as not having happened.
            succeeded = true;

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
        // StructuresResolved is deliberately three-valued. Null is a deployment with no triple store,
        // where the facet was never populated and a rebuild should carry on. False is a triple store
        // that would not answer, where publishing would replace a populated facet with an empty one
        // and delete the index that held it.
        if (report.StructuresResolved == false)
        {
            throw new InvalidOperationException(
                $"The {what} pass could not read the dataset structures, so the prepared indices were "
                + "discarded rather than published over a populated Structures facet.");
        }

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
