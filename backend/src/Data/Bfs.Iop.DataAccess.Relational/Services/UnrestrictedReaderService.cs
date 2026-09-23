using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class UnrestrictedReaderService : IUnrestrictedReaderService
{
    private readonly IopDbContext _dbContext;
    private readonly IVocabulariesService _vocabulariesService;

    public UnrestrictedReaderService(
        IopDbContext dbContext,
        IVocabulariesService vocabulariesService)
    {
        _dbContext = dbContext;
        _vocabulariesService = vocabulariesService;
    }

    public async Task<AgentModel?> TryGetAgentAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetAgentsQuery(asNoTracking: true, EntityIncludeLevel.All, x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToAgentModel(_vocabulariesService);
    }

    public async Task<DataServiceModel?> TryGetDataServiceAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetDataServicesQuery(
            asNoTracking: true,
            EntityIncludeLevel.All, 
            loadPersonalInformation: true, 
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToDataServiceModel(_vocabulariesService);
    }

    public async Task<DcatDatasetModel?> TryGetDatasetAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetDatasetsQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            loadPersonalInformation: true,
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToDcatDatasetModel(_vocabulariesService);
    }

    public async Task<DcatCatalogModel?> TryGetDcatCatalogAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetDcatCatalogsQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToDcatCatalogModel(_vocabulariesService);
    }

    public async Task<IReadOnlyCollection<DcatCatalogRecordModel>> TryGetDcatCatalogRecordsAsync(Guid dcatCatalogId, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetDcatCatalogRecordsQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            x => x.DcatCatalogId == dcatCatalogId);

        var entities = await query.ToListAsync(cancellationToken);

        return entities.Select(x => x.MapToDcatCatalogRecordModel(_vocabulariesService)).ToList().AsReadOnly();
    }

    public async Task<IopConceptModel?> TryGetIopConceptAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetIopConceptsQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            loadPersonalInformation: true,
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToIopConceptModel(_vocabulariesService);
    }

    public async Task<IReadOnlyCollection<CodeListEntryModel>> TryGetCodeListEntriesAsync(Guid conceptId, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetCodeListEntriesQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            x => x.IopConceptId == conceptId);

        var entities = await query.ToListAsync(cancellationToken);

        return entities.Select(x => x.MapToCodeListEntryModel()).ToList().AsReadOnly();
    }

    public async Task<MappingTableModel?> TryGetMappingTableAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetMappingTablesQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            loadPersonalInformation: true,
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToMappingTableModel(_vocabulariesService);
    }

    public async Task<IReadOnlyCollection<MappingRelationModel>> TryGetMappingRelationsAsync(Guid mappingTableid, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetMappingRelationsQuery(
            asNoTracking: true,
            x => x.MappingTableId == mappingTableid);

        var entities = await query.ToListAsync(cancellationToken);

        return entities.Select(x => x.MapToMappingRelationModel(_vocabulariesService)).ToList().AsReadOnly();
    }

    public async Task<PublicServiceModel?> TryGetPublicServiceAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = _dbContext.CreateGetPublicServicesQuery(
            asNoTracking: true,
            EntityIncludeLevel.All,
            loadPersonalInformation: true,
            x => x.Id == id);

        var entity = await query.SingleOrDefaultAsync(cancellationToken);

        return entity?.MapToPublicServiceModel(_vocabulariesService);
    }
}
