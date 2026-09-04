using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class ResourceRelationsService : IResourceRelationsService
{
    private readonly IopDbContext _dbContext;
    private readonly IPublishableEntityAuthorizationService _entityAuthorizationService;

    public ResourceRelationsService(
        IopDbContext dbContext,
        IPublishableEntityAuthorizationService entityAuthorizationService)
    {
        _dbContext = dbContext;
        _entityAuthorizationService = entityAuthorizationService;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetDataServicesReferencedByDatasetsCount(IReadOnlyCollection<Guid> dataServiceIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dataServiceIds, nameof(dataServiceIds));

        if (dataServiceIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedDatasets = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.Datasets);

        var counts = await(
            from relation in _dbContext.DataServiceDatasetRelations
            where dataServiceIds.Contains(relation.DataServiceId)
            join dataset in authorizedDatasets on relation.DatasetId equals dataset.Id
            group dataset.Id by relation.DataServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetDatasetsServedByDataServicesCount
        (
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datasetIds, nameof(datasetIds));

        if (datasetIds.Count == 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedDataServices = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.DataServices);

        var counts = await (
            from relation in _dbContext.DataServiceDatasetRelations
            where datasetIds.Contains(relation.DatasetId)
            join dataService in authorizedDataServices on relation.DataServiceId equals dataService.Id
            group dataService.Id by relation.DatasetId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesReferencedInRelationsCount(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        if (publicServiceIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedPublicServices = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.PublicServices);

        var linkedToCounts = await (
            from relation in _dbContext.PublicServicesRelations
            where publicServiceIds.Contains(relation.PublicServiceId)
            join target in authorizedPublicServices on relation.RelationId equals target.Id
            group target.Id by relation.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return linkedToCounts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesReferencedInRequiresCount(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        if (publicServiceIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedPublicServices = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.PublicServices);

        var requiresCounts = await (
            from requires in _dbContext.Set<PublicServiceRequires>()
            where publicServiceIds.Contains(requires.PublicServiceId)
            join target in authorizedPublicServices on requires.RequiresId equals target.Id
            group target.Id by requires.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return requiresCounts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesDescribedAtDatasetsCount(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publicServiceIds, nameof(publicServiceIds));

        if (publicServiceIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedDatasets = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.Datasets);

        var counts = await (
            from relation in _dbContext.PublicServiceDescribedAtDatasetRelations
            where publicServiceIds.Contains(relation.PublicServiceId)
            join dataset in authorizedDatasets on relation.IsDescribedAtId equals dataset.Id
            group dataset.Id by relation.PublicServiceId into grouped
            select new { Id = grouped.Key, Count = grouped.Select(x => x).Distinct().Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetMappingTablesReferencedInConformsToCount(
        IReadOnlyCollection<Guid> mappingTableIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappingTableIds, nameof(mappingTableIds));

        if (mappingTableIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var counts = await _dbContext.Set<Resource>()
            .Where(r => r.MappingTableConformsToId != null && mappingTableIds.Contains(r.MappingTableConformsToId.Value))
            .GroupBy(r => r.MappingTableConformsToId!.Value)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);

        return counts.AsReadOnly();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetConceptsReferencedInMappingTablesCount(
        IReadOnlyDictionary<string, Guid> conceptsIrisAndIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conceptsIrisAndIds, nameof(conceptsIrisAndIds));

        if (conceptsIrisAndIds.Count is 0)
        {
            return new Dictionary<Guid, int>().AsReadOnly();
        }

        var authorizedMappingTables = _entityAuthorizationService.AppendUserReadAuthorizationConditionToDatabaseQuery(_dbContext.MappingTables);

        var allIris = conceptsIrisAndIds.Keys.ToList();

        // Fetch the authorized mapping tables that reference any of the concept IRIs on either end.
        var matches = await authorizedMappingTables
            .Where(mt => allIris.Contains(mt.SourceUri) || allIris.Contains(mt.TargetUri))
            .Select(mt => new { mt.SourceUri, mt.TargetUri })
            .ToListAsync(cancellationToken);

        // Count each mapping table once per concept, even when both source and target
        // point to the same concept IRI.
        return conceptsIrisAndIds
            .ToDictionary(
                kv => kv.Value,
                kv => matches.Count(m => m.SourceUri == kv.Key || m.TargetUri == kv.Key))
            .AsReadOnly();
    }
}
