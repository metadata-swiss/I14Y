using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.Hosting;

public sealed class ReindexOrchestrator
{
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
        _options = options.Value;
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
                    .RebuildAsync(_options.ReindexBatchSize, cancellationToken);

                codeLists = await provider.GetRequiredService<CodeListIndexRebuilder>()
                    .RebuildAsync(_options.ReindexBatchSize, cancellationToken);

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

            await provisioner.ForceMergeAsync(cancellationToken);

            succeeded = true;

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
            _logger.LogError(exception, "A {Operation} failed. The live indices are unchanged.", operation);
        }
        finally
        {
            _gate.End(succeeded, catalog, codeLists);
        }
    }

    // Whether the structures were read is not the question; whether this pass would destroy them is.
    // A pass that could not read them writes no flags at all, and the swap then deletes the generation
    // that had them. Asking the live index is the only way to tell an always-empty facet from one this
    // pass is about to empty — a triple store removed or misconfigured after an earlier good pass
    // reports "not configured", which on its own looks innocent.
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
