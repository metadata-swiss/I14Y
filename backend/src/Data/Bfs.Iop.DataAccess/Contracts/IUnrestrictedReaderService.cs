using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

/// <summary>
/// Defines a service where entities can be read by bypassing the authorization security.
/// These services should only be use for background tasks.
/// </summary>
public interface IUnrestrictedReaderService
{
    Task<AgentModel?> TryGetAgentAsync(Guid id, CancellationToken cancellationToken);

    Task<DataServiceModel?> TryGetDataServiceAsync(Guid id, CancellationToken cancellationToken);

    Task<DcatDatasetModel?> TryGetDatasetAsync(Guid id, CancellationToken cancellationToken);

    Task<DcatCatalogModel?> TryGetDcatCatalogAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DcatCatalogRecordModel>> TryGetDcatCatalogRecordsAsync(Guid dcatCatalogId, CancellationToken cancellationToken);

    Task<IopConceptModel?> TryGetIopConceptAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CodeListEntryModel>> TryGetCodeListEntriesAsync(Guid conceptId, CancellationToken cancellationToken);

    Task<MappingTableModel?> TryGetMappingTableAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MappingRelationModel>> TryGetMappingRelationsAsync(Guid mappingTableid, CancellationToken cancellationToken);

    Task<PublicServiceModel?> TryGetPublicServiceAsync(Guid id, CancellationToken cancellationToken);
}

