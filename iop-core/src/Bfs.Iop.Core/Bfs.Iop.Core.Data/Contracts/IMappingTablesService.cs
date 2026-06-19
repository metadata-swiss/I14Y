using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IMappingTablesService : IPublishableEntityService
{
    Task<MappingTableModel> GetMappingTable(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<MappingTableModel>> GetMappingTables(
        string? mappingTableIdentifier,
        string? publisherIdentifier,
        string? version,
        string? codeSystemUri,
        PublicationLevel? publicationLevel,
        RegistrationStatus? registrationStatus,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<IEnumerable<MappingTableModel>> GetMappingTablesForIndexInBatches(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    Task<IdentifierVersionExistsModel> GetIdentifierVersionExists(
        string identifier,
        string version,
        CancellationToken cancellationToken = default);

    Task<MappingRelationModel> GetMappingRelation(Guid id, Guid mappingRelationId, CancellationToken cancellationToken = default);

    Task<PagedResult<MappingRelationModel>> GetMappingRelations(
        Guid mappingTableId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Guid> AddMappingTable(MappingTableInputModel inputModel, CancellationToken cancellationToken = default);

    Task<Guid> AddMappingTableVersion(Guid previousId, MappingTableInputModel inputModel, CancellationToken cancellationToken = default);

    Task<IEnumerable<Guid>> AddRelations(
        Guid mappingTableId,
        IEnumerable<MappingRelationInputModel> inputModels,
        CancellationToken cancellationToken = default);

    Task UpdateMappingTable(Guid id, MappingTableInputModel updateModel, CancellationToken cancellationToken = default);

    Task UpdateMappingRelation(
        Guid mappingTableId,
        Guid mappingRelationId,
        MappingRelationInputModel updateModel,
        CancellationToken cancellationToken = default);
    
    Task DeleteMappingTable(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAllMappingRelations(Guid mappingTableId, CancellationToken cancellationToken = default);

    Task DeleteMappingRelation(Guid mappingTableId, Guid mappingRelationId, CancellationToken cancellationToken = default);
}
