using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class ResourceDataReaderService : IResourceDataReaderService
{
    private readonly IUnrestrictedReaderService _unrestrictedDbReaderService;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;

    public ResourceDataReaderService(
        IUnrestrictedReaderService unrestrictedReaderService,
        IDatasetModelProcessService datasetModelProcessService)
    {
        _unrestrictedDbReaderService = unrestrictedReaderService;
        _datasetModelFileProcessService = datasetModelProcessService;
    }

    public async Task<Stream> GetResourceDataAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken)
    {
        return resourceType switch
        {
            AuditTrailResourceType.Agent => await TryGetAgentDataAsync(id, cancellationToken),
            AuditTrailResourceType.Concept => await TryGetConceptDataAsync(id, cancellationToken),
            AuditTrailResourceType.DcatCatalog => await TryGetDcatCatalogDataAsync(id, cancellationToken),
            AuditTrailResourceType.Dataset => await TryGetDatasetDataAsync(id, cancellationToken),
            AuditTrailResourceType.DataService => await TryGetDataServiceDataAsync(id, cancellationToken),
            AuditTrailResourceType.MappingTable => await TryGetMappingTableDataAsync(id, cancellationToken),
            AuditTrailResourceType.PublicService => await TryGetPublicServiceDataAsync(id, cancellationToken),
            AuditTrailResourceType.ConceptCodeListEntries => await TryGetCodeListEntriesDataAsync(id, cancellationToken),
            AuditTrailResourceType.DcatCatalogRecords => await TryGetDcatCatalogRecordsDataAsync(id, cancellationToken),
            AuditTrailResourceType.DatasetStructure => await TryGetDatasetStructureAsync(id, cancellationToken),
            AuditTrailResourceType.MappingTableRelations => await TryGetMappingRelationsDataAsync(id, cancellationToken),
            _ => throw new NotImplementedException()
        } ?? throw new NotFoundException($"The resource of type '{resourceType}' and id {id}' was not found.");
    }

    private async Task<Stream?> TryGetDatasetStructureAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _datasetModelFileProcessService.GraphExists(id, cancellationToken))
        {
            return null;
        }

        var resource = await _datasetModelFileProcessService.ExportGraph(LinkedDataFormat.Ttl, id, cancellationToken);

        return resource.Data;
    }

    private async Task<Stream?> TryGetAgentDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetAgentAsync(id, cancellationToken);

        return resource is not null
            ? IopJsonSerializer.Serialize(resource)
            : null;
    }

    private async Task<Stream?> TryGetDcatCatalogDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetDcatCatalogAsync(id, cancellationToken);

        return resource is not null
            ? IopJsonSerializer.Serialize(resource)
            : null;
    }

    private async Task<Stream?> TryGetDcatCatalogRecordsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var catalog = await _unrestrictedDbReaderService.TryGetDcatCatalogAsync(id, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetDcatCatalogRecordsAsync(id, cancellationToken);

        return catalog is not null
            ? IopJsonSerializer.Serialize(resource)
            : null;
    }

    private async Task<Stream?> TryGetDataServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetDataServiceAsync(id, cancellationToken);

        return resource is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }

    private async Task<Stream?> TryGetDatasetDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetDatasetAsync(id, cancellationToken);

        return resource is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }

    private async Task<Stream?> TryGetConceptDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetIopConceptAsync(id, cancellationToken);

        return resource is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }

    private async Task<Stream?> TryGetCodeListEntriesDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var concept = await _unrestrictedDbReaderService.TryGetIopConceptAsync(id, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetCodeListEntriesAsync(id, cancellationToken);

        return concept is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }

    private async Task<Stream?> TryGetMappingTableDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetMappingTableAsync(id, cancellationToken);

        return resource is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }

    private async Task<Stream?> TryGetMappingRelationsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var mappingTable = await _unrestrictedDbReaderService.TryGetMappingTableAsync(id, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetMappingRelationsAsync(id, cancellationToken);

        return mappingTable is not null
            ? IopJsonSerializer.Serialize(resource)
            : null;
    }

    private async Task<Stream?> TryGetPublicServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedDbReaderService.TryGetPublicServiceAsync(id, cancellationToken);

        return resource is not null
             ? IopJsonSerializer.Serialize(resource)
             : null;
    }
}
