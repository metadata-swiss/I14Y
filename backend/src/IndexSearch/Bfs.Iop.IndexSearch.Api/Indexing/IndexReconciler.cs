using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>Applies a batch of index events to Elasticsearch.</summary>
public interface IIndexReconciler
{
    Task ReconcileAsync(IReadOnlyCollection<IndexEvent> events, CancellationToken cancellationToken);
}

/// <summary>
/// Writes the payloads it was given and removes the documents whose payload is null.
/// <para>
/// No database access: the sender owns the data, which keeps this service out of the
/// read-authorization question entirely (see <see cref="IndexEvent"/>). The only component that
/// reads Postgres is the full index builder, which uses the existing authorization-free
/// <c>Get*ForIndexInBatches</c> contracts.
/// </para>
/// </summary>
internal sealed class IndexReconciler : IIndexReconciler
{
    private readonly ICatalogIndexService _catalogIndex;
    private readonly ICodeListEntryIndexService _codeListIndex;
    private readonly ILogger<IndexReconciler> _logger;

    public IndexReconciler(
        ICatalogIndexService catalogIndex,
        ICodeListEntryIndexService codeListIndex,
        ILogger<IndexReconciler> logger)
    {
        _catalogIndex = catalogIndex;
        _codeListIndex = codeListIndex;
        _logger = logger;
    }

    public async Task ReconcileAsync(IReadOnlyCollection<IndexEvent> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);

        try
        {
            await RemoveAsync(events, cancellationToken);
            await IndexAsync(events, cancellationToken);
        }
        catch (Exception ex)
        {
            // Surface it, but never let it end the worker loop; the periodic full rebuild repairs
            // whatever this batch failed to write.
            _logger.LogError(ex, "Failed to apply a batch of {Count} index event(s).", events.Count);
            throw;
        }
    }

    private async Task RemoveAsync(IReadOnlyCollection<IndexEvent> events, CancellationToken cancellationToken)
    {
        var catalogIds = events
            .Where(x => x.Payload is null && x.Target is IndexTarget.Catalog)
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        if (catalogIds.Count > 0)
        {
            await _catalogIndex.DeIndexAsync(catalogIds, cancellationToken);
        }

        var codeListIds = events
            .Where(x => x.Payload is null && x.Target is IndexTarget.CodeListEntry)
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        if (codeListIds.Count > 0)
        {
            await _codeListIndex.DeIndexAsync(codeListIds, cancellationToken);
        }
    }

    private async Task IndexAsync(IReadOnlyCollection<IndexEvent> events, CancellationToken cancellationToken)
    {
        var payloads = events.Where(x => x.Payload is not null).Select(x => x.Payload!).ToList();
        if (payloads.Count == 0)
        {
            return;
        }

        // Group by model type so each engine call is a single bulk request.
        await IndexTypedAsync<DcatDatasetModel>(payloads, async models =>
        {
            // hasStructure is not part of the model; passing null keeps whatever the last full
            // build determined from the triple store instead of clearing it.
            foreach (var model in models)
            {
                await _catalogIndex.UpdateIndexAsync(model, hasStructure: null, cancellationToken);
            }
        });

        await IndexTypedAsync<DataServiceModel>(payloads, models => _catalogIndex.UpdateIndexAsync(models, cancellationToken));
        await IndexTypedAsync<PublicServiceModel>(payloads, models => _catalogIndex.UpdateIndexAsync(models, cancellationToken));
        await IndexTypedAsync<IopConceptModel>(payloads, models => _catalogIndex.UpdateIndexAsync(models, cancellationToken));
        await IndexTypedAsync<MappingTableModel>(payloads, models => _catalogIndex.UpdateIndexAsync(models, cancellationToken));
        await IndexTypedAsync<CodeListEntryModel>(payloads, models => _codeListIndex.UpdateIndexAsync(models, cancellationToken));

        var unknown = payloads.Where(x => x is not (
            DcatDatasetModel or DataServiceModel or PublicServiceModel or
            IopConceptModel or MappingTableModel or CodeListEntryModel)).ToList();

        if (unknown.Count > 0)
        {
            _logger.LogWarning(
                "Ignored {Count} index payload(s) of unsupported type(s): {Types}.",
                unknown.Count,
                string.Join(", ", unknown.Select(x => x.GetType().Name).Distinct()));
        }
    }

    private static Task IndexTypedAsync<T>(List<object> payloads, Func<List<T>, Task> index)
    {
        var typed = payloads.OfType<T>().ToList();
        return typed.Count == 0 ? Task.CompletedTask : index(typed);
    }
}
