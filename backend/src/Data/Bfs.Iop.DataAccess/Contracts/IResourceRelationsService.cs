namespace Bfs.Iop.DataAccess.Contracts;

public interface IResourceRelationsService
{
    Task<IReadOnlyDictionary<Guid, int>> GetDatasetsServedByDataServicesCount(
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetDataServicesReferencedByDatasetsCount(
            IReadOnlyCollection<Guid> dataServiceIds,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesReferencedInRelationsCount(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesReferencedInRequiresCount(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetPublicServicesDescribedAtDatasetsCount(
        IReadOnlyCollection<Guid> publicServicesIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetMappingTablesReferencedInConformsToCount(
        IReadOnlyCollection<Guid> mappingTableIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> GetConceptsReferencedInMappingTablesCount(
        IReadOnlyDictionary<string, Guid> conceptsIrisAndIds,
        CancellationToken cancellationToken = default);
}
