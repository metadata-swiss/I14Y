using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Extensions;
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
        resourceType.EnsureValueIsValid();

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
            _ => throw new NotSupportedException($"The resource type '{resourceType}' is not supported.")
        } ?? throw new NotFoundException($"The resource of type '{resourceType}' and id {id}' was not found.");
    }

    private async Task<Stream?> TryGetDatasetStructureAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _datasetModelFileProcessService.GraphExists(id, cancellationToken))
        {
            return null;
        }

        using var resource = await _datasetModelFileProcessService.ExportGraph(LinkedDataFormat.Ttl, id, cancellationToken);

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

    private async Task<Stream?> TryGetDcatCatalogRecordsDataAsync(Guid dcatCatalogId, CancellationToken cancellationToken)
    {
        var catalog = await _unrestrictedDbReaderService.TryGetDcatCatalogAsync(dcatCatalogId, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetDcatCatalogRecordsAsync(dcatCatalogId, cancellationToken);

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

    private async Task<Stream?> TryGetCodeListEntriesDataAsync(Guid conceptId, CancellationToken cancellationToken)
    {
        var concept = await _unrestrictedDbReaderService.TryGetIopConceptAsync(conceptId, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetCodeListEntriesAsync(conceptId, cancellationToken);

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

    private async Task<Stream?> TryGetMappingRelationsDataAsync(Guid mappingTableId, CancellationToken cancellationToken)
    {
        var mappingTable = await _unrestrictedDbReaderService.TryGetMappingTableAsync(mappingTableId, cancellationToken);
        var resource = await _unrestrictedDbReaderService.TryGetMappingRelationsAsync(mappingTableId, cancellationToken);

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
