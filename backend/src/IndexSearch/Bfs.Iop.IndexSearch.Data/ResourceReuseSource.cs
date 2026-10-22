using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class ResourceReuseSource : IResourceReuseSource
{
    // One SPARQL VALUES clause per call. Big enough that the whole catalogue takes a few round
    // trips, small enough that no single query string grows with the number of concepts.
    private const int ConceptsPerQuery = 500;

    private readonly ISearchIndexProviderService _provider;
    private readonly IDatasetModelProcessService _structures;
    private readonly ILogger<ResourceReuseSource> _logger;

    public ResourceReuseSource(
        ISearchIndexProviderService provider,
        IDatasetModelProcessService structures,
        ILogger<ResourceReuseSource> logger)
    {
        _provider = provider;
        _structures = structures;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Guid, int>?> GetReuseCountsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var counts = new Dictionary<Guid, int>(await _provider.GetPublicReuseCounts(cancellationToken));

            await AddConceptStructureReferencesAsync(counts, cancellationToken);

            return counts;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            // Half a set of counts would rank a concept below one that is reused less, which is the
            // opposite of what the field exists for, so a failure yields nothing at all.
            _logger.LogError(ex, "The reuse counts could not be read, so none were applied.");

            return null;
        }
    }

    private async Task AddConceptStructureReferencesAsync(
        Dictionary<Guid, int> counts,
        CancellationToken cancellationToken)
    {
        var concepts = await _provider.GetConceptIdentities(cancellationToken);

        if (concepts.Count == 0)
        {
            return;
        }

        // The triple store records which datasets conform to a concept but knows nothing of
        // publication levels, so the references come back whole and are filtered here.
        var publicDatasetIds = await _provider.GetPublicDatasetIds(cancellationToken);

        foreach (var chunk in concepts.Chunk(ConceptsPerQuery))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var references = await _structures.GetConceptStructureReferencesBatch(
                chunk.Select(x => new IopConceptData(x.Id, x.Identifier, x.Version)),
                cancellationToken);

            foreach (var (conceptId, datasetIds) in references)
            {
                var reused = datasetIds.Count(publicDatasetIds.Contains);

                if (reused > 0)
                {
                    counts[conceptId] = counts.GetValueOrDefault(conceptId) + reused;
                }
            }
        }
    }
}
