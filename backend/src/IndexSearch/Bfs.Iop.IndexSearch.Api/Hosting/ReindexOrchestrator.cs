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
            }
            catch
            {
                await provisioner.DiscardAsync(prepared, CancellationToken.None);
                throw;
            }

            await provisioner.PublishAsync(prepared, cancellationToken);

            if (_options.ForceMergeAfterReindex)
            {
                await provisioner.ForceMergeAsync(cancellationToken);
            }

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
