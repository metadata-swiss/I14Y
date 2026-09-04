using Bfs.Iop.Common.Options;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Tools;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Services;

internal sealed class RelationsCountService : IRelationsCountService
{
    private readonly IResourceRelationsService _resourceRelationsService;

    private readonly IDatasetsService _datasetsService;
    private readonly IIopConceptsService _iopConceptsService;

    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly string _baseIriUrl;

    public RelationsCountService(
        IResourceRelationsService resourceRelationsService,
        IDatasetsService datasetsService,
        IIopConceptsService iopConceptsService,
        IDatasetModelProcessService datasetModelProcessService,
        IOptions<I14YOptions> i14yOptions)
    {
        _resourceRelationsService = resourceRelationsService ?? throw new ArgumentNullException(nameof(resourceRelationsService));
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _iopConceptsService = iopConceptsService ?? throw new ArgumentNullException(nameof(iopConceptsService));

        _datasetModelProcessService = datasetModelProcessService ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
        ArgumentNullException.ThrowIfNull(i14yOptions, nameof(i14yOptions));
        _baseIriUrl = i14yOptions.Value.IriBaseUrl.TrimEnd('/');
    }

    private sealed record ConceptIdentifierModel(Guid Id, string Identifier, string Version);

    public Task<IReadOnlyDictionary<Guid, int>> GetDatasetServedByDataServiceCountBatch(
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datasetIds, nameof(datasetIds));

        return _resourceRelationsService.GetDatasetsServedByDataServicesCount(datasetIds, cancellationToken);
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetDataServiceReferencedByDatasetCountBatch(
        IReadOnlyCollection<Guid> dataServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataServiceIds, nameof(dataServiceIds));

        return _resourceRelationsService.GetDataServicesReferencedByDatasetsCount(dataServiceIds, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceReferencedByCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        var linkedToCounts = await _resourceRelationsService.GetPublicServicesReferencedInRelationsCount(publicServiceIds, cancellationToken);

        var requiresCounts = await _resourceRelationsService.GetPublicServicesReferencedInRequiresCount(publicServiceIds, cancellationToken);

        var totals = new Dictionary<Guid, int>(linkedToCounts);
        foreach (var (id, count) in requiresCounts)
        {
            totals[id] = totals.GetValueOrDefault(id) + count;
        }

        return totals.AsReadOnly();
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceDescribesDatasetCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        return _resourceRelationsService.GetPublicServicesDescribedAtDatasetsCount(publicServiceIds, cancellationToken);
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetMappingTableReferencedByCountBatch(
        IReadOnlyCollection<Guid> mappingTableIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappingTableIds, nameof(mappingTableIds));

        return _resourceRelationsService.GetMappingTablesReferencedInConformsToCount(mappingTableIds, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetConceptStructureReferenceCountBatch(
        IReadOnlyCollection<Guid> conceptIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conceptIds, nameof(conceptIds));

        var concepts = await ResolveConceptIdentifiers(conceptIds, cancellationToken);
        if (concepts.Count == 0)
        {
            return Empty;
        }

        // Get the (concept → datasetId) references from the triple store, then keep only the
        // references to datasets the current user may read (same read-auth as the catalogue search).
        var conceptData = concepts.Select(c => new IopConceptData(c.Id, c.Identifier, c.Version));
        var references = await _datasetModelProcessService.GetConceptStructureReferencesBatch(conceptData, cancellationToken);
        if (references.Count == 0)
        {
            return Empty;
        }

        var allDatasetIds = references.Values.SelectMany(x => x).Distinct().ToList();
        var authorizedDatasetIds = (await _datasetsService.GetDatasets(allDatasetIds, cancellationToken)).Select(x => x.Id).ToList();

        return references
            .ToDictionary(kv => kv.Key, kv => kv.Value.Count(authorizedDatasetIds.Contains))
            .AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetConceptMappingTableCountBatch(
        IReadOnlyCollection<Guid> conceptIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conceptIds, nameof(conceptIds));

        var concepts = await ResolveConceptIdentifiers(conceptIds, cancellationToken);

        var iriToId = new Dictionary<string, Guid>();
        foreach (var concept in concepts)
        {
            iriToId[IriHelper.BuildConceptIri(_baseIriUrl, concept.Identifier, concept.Version)] = concept.Id;
        }

        if (iriToId.Count == 0)
        {
            return Empty;
        }

        return await _resourceRelationsService.GetConceptsReferencedInMappingTablesCount(iriToId, cancellationToken);
    }

    /// <summary>Resolves identifier + version for the given concept ids; skips concepts with none.</summary>
    private async Task<IReadOnlyList<ConceptIdentifierModel>> ResolveConceptIdentifiers(
        IReadOnlyCollection<Guid> conceptIds,
        CancellationToken cancellationToken)
    {
        var ids = conceptIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var concepts = (await _iopConceptsService.GetIopConcepts(ids, cancellationToken));

        return concepts
            .Where(r => r.Identifiers.Any()
                && !string.IsNullOrWhiteSpace(r.Identifiers.First())
                && !string.IsNullOrWhiteSpace(r.Version))
            .Select(r => new ConceptIdentifierModel(r.Id, r.Identifiers.First(), r.Version))
            .ToList().AsReadOnly();
    }

    private static IReadOnlyDictionary<Guid, int> Empty => new Dictionary<Guid, int>().AsReadOnly();
}
