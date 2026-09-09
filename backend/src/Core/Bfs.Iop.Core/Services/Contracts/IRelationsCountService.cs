namespace Bfs.Iop.Core.Services.Contracts;

/// <summary>
/// Computes the "relations" count shown on the catalogue, matching what each resource's detail page
/// lists. Concepts and mapping tables are counted by incoming references; data services and public
/// services are counted by their own declared (outgoing) relations.
/// Each method runs a single grouped query for the whole batch and applies the same
/// read-authorization as the catalogue search. The methods must not be invoked concurrently on
/// the same scoped <c>DbContext</c>.
/// </summary>
internal interface IRelationsCountService
{
    /// <summary>Per dataset: number of distinct data services serving it (matches the detail "Is served by" section).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetDatasetServedByDataServiceCountBatch(
        IReadOnlyCollection<Guid> datasetIds,
        CancellationToken cancellationToken = default);

    /// <summary>Per data service: number of distinct datasets it declares it serves (matches the detail "Serves Dataset" section).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetDataServiceReferencedByDatasetCountBatch(
        IReadOnlyCollection<Guid> dataServiceIds,
        CancellationToken cancellationToken = default);

    /// <summary>Per public service: number of distinct services it declares it relates to or requires (matches the detail "Is linked to" + "Requires" sections).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceReferencedByCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default);

    /// <summary>Per public service: number of distinct datasets it declares it is described at (matches the detail "Is described at" section).</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetPublicServiceDescribesDatasetCountBatch(
        IReadOnlyCollection<Guid> publicServiceIds,
        CancellationToken cancellationToken = default);

    /// <summary>Per mapping table: number of resources conforming to it.</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetMappingTableReferencedByCountBatch(
        IReadOnlyCollection<Guid> mappingTableIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Per concept: number of structure attributes (property shapes) conforming to the concept,
    /// counted only across datasets the current user is authorized to read.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetConceptStructureReferenceCountBatch(
        IReadOnlyCollection<Guid> conceptIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Per concept: number of distinct (authorized) mapping tables whose source or target URI
    /// equals the concept IRI. A mapping table matching the concept on both ends is counted once.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetConceptMappingTableCountBatch(
        IReadOnlyCollection<Guid> conceptIds,
        CancellationToken cancellationToken = default);
}
