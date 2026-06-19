using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

internal sealed class RelationsCountService : IRelationsCountService
{
    private readonly IopDbContext _dbContext;
    private readonly IPublishableEntityAuthorizationService _authorizationService;
    private readonly IDatasetModelProcessService _datasetModelProcessService;
    private readonly string _baseIriUrl;

    public RelationsCountService(
        IopDbContext dbContext,
        IPublishableEntityAuthorizationService authorizationService,
        IDatasetModelProcessService datasetModelProcessService,
        IOptions<I14YOptions> i14yOptions)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _datasetModelProcessService = datasetModelProcessService ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
        ArgumentNullException.ThrowIfNull(i14yOptions, nameof(i14yOptions));
        _baseIriUrl = i14yOptions.Value.IriBaseUrl.TrimEnd('/');
    }

    private sealed record ConceptIdentifierModel(Guid Id, string Identifier, string Version);

    public async Task<IReadOnlyDictionary<Guid, int>> GetDatasetServedByDataServiceCountBatch(
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datasetIds, nameof(datasetIds));

        var ids = datasetIds.ToList();
        if (ids.Count == 0)
        {
            return Empty;
        }

        var authorizedDataServices = _authorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.DataServices);

        var counts = await (
            from relation in _dbContext.DataServiceDatasetRelations
            where ids.Contains(relation.DatasetId)
            join dataService in authorizedDataServices on relation.DataServiceId equals dataService.Id
            group dataService.Id by relation.DatasetId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetDataServiceReferencedByDatasetCountBatch(
        IReadOnlyCollection<Guid> dataServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataServiceIds, nameof(dataServiceIds));

        var ids = dataServiceIds.ToList();
        if (ids.Count == 0)
        {
            return Empty;
        }

        // Count the data service's own declared datasets ("serves dataset"), matching the detail
        // page, restricted to the datasets the current user may read.
        var authorizedDatasets = _authorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.Datasets);

        var counts = await (
            from relation in _dbContext.DataServiceDatasetRelations
            where ids.Contains(relation.DataServiceId)
            join dataset in authorizedDatasets on relation.DatasetId equals dataset.Id
            group dataset.Id by relation.DataServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceReferencedByCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        var ids = publicServiceIds.ToList();
        if (ids.Count == 0)
        {
            return Empty;
        }

        // Count the public service's own declared relations ("relation" + "requires"), matching the
        // detail page, restricted to the target services the current user may read.
        var authorizedPublicServices = _authorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.PublicServices);

        var linkedToCounts = await (
            from relation in _dbContext.PublicServicesRelations
            where ids.Contains(relation.PublicServiceId)
            join target in authorizedPublicServices on relation.RelationId equals target.Id
            group target.Id by relation.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        var requiresCounts = await (
            from requires in _dbContext.Set<PublicServiceRequires>()
            where ids.Contains(requires.PublicServiceId)
            join target in authorizedPublicServices on requires.RequiresId equals target.Id
            group target.Id by requires.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        var totals = new Dictionary<Guid, int>(linkedToCounts);
        foreach (var (id, count) in requiresCounts)
        {
            totals[id] = totals.GetValueOrDefault(id) + count;
        }

        return totals.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceDescribesDatasetCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        var ids = publicServiceIds.ToList();
        if (ids.Count == 0)
        {
            return Empty;
        }

        // Count the datasets the public service declares it is described at ("is described at"),
        // matching the detail page, restricted to the datasets the current user may read.
        var authorizedDatasets = _authorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.Datasets);

        var counts = await (
            from relation in _dbContext.PublicServiceDescribedAtDatasetRelations
            where ids.Contains(relation.PublicServiceId)
            join dataset in authorizedDatasets on relation.IsDescribedAtId equals dataset.Id
            group dataset.Id by relation.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetMappingTableReferencedByCountBatch(
        IReadOnlyCollection<Guid> mappingTableIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappingTableIds, nameof(mappingTableIds));

        var ids = mappingTableIds.ToList();
        if (ids.Count == 0)
        {
            return Empty;
        }

        var counts = await _dbContext.Set<Resource>()
            .Where(r => r.MappingTableConformsToId != null && ids.Contains(r.MappingTableConformsToId.Value))
            .GroupBy(r => r.MappingTableConformsToId!.Value)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
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
        var authorizedDatasetIds = await GetAuthorizedDatasetIds(allDatasetIds, cancellationToken);

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

        var allIris = iriToId.Keys.ToList();

        var authorizedMappingTables = _authorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.MappingTables);

        // Fetch the authorized mapping tables that reference any of the concept IRIs on either end.
        var matches = await authorizedMappingTables
            .Where(mt => allIris.Contains(mt.SourceUri) || allIris.Contains(mt.TargetUri))
            .Select(mt => new { mt.SourceUri, mt.TargetUri })
            .ToListAsync(cancellationToken);

        // Count each mapping table once per concept, even when both source and target
        // point to the same concept IRI.
        return iriToId
            .ToDictionary(
                kv => kv.Value,
                kv => matches.Count(m => m.SourceUri == kv.Key || m.TargetUri == kv.Key))
            .AsReadOnly();
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

        var items = await _dbContext.IopConcepts
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Identifiers, c.Version })
            .ToListAsync(cancellationToken);

        return items
            .Where(r => r.Identifiers.Length > 0
                && !string.IsNullOrWhiteSpace(r.Identifiers[0])
                && !string.IsNullOrWhiteSpace(r.Version))
            .Select(r => new ConceptIdentifierModel(r.Id, r.Identifiers[0], r.Version))
            .ToList().AsReadOnly();
    }

    /// <summary>Returns the subset of dataset ids the current user is authorized to read.</summary>
    private async Task<IReadOnlySet<Guid>> GetAuthorizedDatasetIds(
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken)
    {
        var ids = datasetIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return EmptyIdSet;
        }

        var authorized = await _authorizationService
            .AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.Datasets)
            .Where(d => ids.Contains(d.Id))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        return authorized.ToHashSet();
    } 

    private static IReadOnlySet<Guid> EmptyIdSet { get; } = new HashSet<Guid>();

    private static IReadOnlyDictionary<Guid, int> Empty => new Dictionary<Guid, int>().AsReadOnly();
}
