using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.LinkedData.Services;
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
    private readonly IDatasetModelProcessService _datasetModels;
    private readonly ILogger<IndexReconciler> _logger;

    public IndexReconciler(
        ICatalogIndexService catalogIndex,
        ICodeListEntryIndexService codeListIndex,
        IDatasetModelProcessService datasetModels,
        ILogger<IndexReconciler> logger)
    {
        _catalogIndex = catalogIndex;
        _codeListIndex = codeListIndex;
        _datasetModels = datasetModels;
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

    /// <summary>
    /// Does this dataset have a structure? Answered from the object store — the same container and
    /// the same file-storage service the full index build lists, so the trigger path and the rebuild
    /// cannot disagree. <c>GraphExists</c> is also authorization-free, which matters because this
    /// runs on a background worker with no HTTP user.
    /// </summary>
    /// <returns>
    /// <c>true</c>/<c>false</c> when the store answered; <c>null</c> when it could not be reached.
    /// <para>
    /// Null on failure is deliberate and NOT laziness: the engine treats null as "keep the value
    /// already indexed". Returning false instead would let a single object-store outage quietly
    /// strip the structure flag from every dataset that happened to be edited during it, and the
    /// only symptom would be an emptying Structures facet.
    /// </para>
    /// </returns>
    private async Task<bool?> ResolveHasStructureAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        try
        {
            return await _datasetModels.GraphExists(datasetId, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Could not read the object store to resolve hasStructure for dataset {DatasetId}; " +
                "keeping the currently indexed value. The next full rebuild will correct it.",
                datasetId);

            return null;
        }
    }

    private async Task IndexAsync(IReadOnlyCollection<IndexEvent> events, CancellationToken cancellationToken)
    {
        var payloads = events.Where(x => x.Payload is not null).Select(x => x.Payload!).ToList();
        if (payloads.Count == 0)
        {
            return;
        }

        var catalog = payloads.OfType<CatalogIndexEntry>().ToList();
        if (catalog.Count > 0)
        {
            // hasStructure cannot ride along on a forward: it lives in the object store, and Core
            // does not read it. Resolve it HERE rather than letting it default.
            //
            // Load-bearing for the two operations that actually change the flag — importing and
            // deleting a dataset model. Without this those datasets keep their previously indexed
            // value until the next full rebuild: a structure you just uploaded would not appear in
            // the Structures facet, and one you just deleted would still be listed. Silently, for
            // hours.
            var resolved = new List<CatalogIndexEntry>(catalog.Count);
            foreach (var entry in catalog)
            {
                resolved.Add(entry.Type is SearchResourceType.Dataset
                    ? entry with { HasStructure = await ResolveHasStructureAsync(entry.Id, cancellationToken) }
                    : entry);
            }

            await _catalogIndex.UpdateIndexAsync(resolved, cancellationToken);
        }

        var codeList = payloads.OfType<CodeListIndexEntry>().ToList();
        if (codeList.Count > 0)
        {
            await _codeListIndex.UpdateIndexAsync(codeList, cancellationToken);
        }

        var unknown = payloads.Where(x => x is not (CatalogIndexEntry or CodeListIndexEntry)).ToList();
        if (unknown.Count > 0)
        {
            _logger.LogWarning(
                "Ignored {Count} index payload(s) of unsupported type(s): {Types}.",
                unknown.Count,
                string.Join(", ", unknown.Select(x => x.GetType().Name).Distinct()));
        }
    }
}
