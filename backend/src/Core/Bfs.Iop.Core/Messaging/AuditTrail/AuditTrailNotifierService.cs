using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Messaging.Queues;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed class AuditTrailNotifierService : IAuditTrailNotifierService
{
    private readonly IAuditTrailApiClient _apiClient;
    private readonly IMessageQueue<AuditTrailMessage> _queue;
    private readonly ILogger<AuditTrailNotifierService> _logger;
    private readonly IUserContextService _userContextService;
    private readonly IUnrestrictedReaderService _unrestrictedReaderService;

    public AuditTrailNotifierService(
        IAuditTrailApiClient auditTrailApiClient,
        IMessageQueue<AuditTrailMessage> queue,
        IUserContextService userContextService,
        IUnrestrictedReaderService unrestrictedReaderService,
        ILogger<AuditTrailNotifierService> logger)
    {
        _apiClient = auditTrailApiClient;
        _queue = queue;
        _userContextService = userContextService;
        _unrestrictedReaderService = unrestrictedReaderService;
        _logger = logger;
    }

    public async Task EnsureResourceIsTrackedAsync(
        AuditTrailResourceType resourceType, 
        Guid id, 
        CancellationToken cancellationToken)
    {
        var metadata = GetResourceMetadata(resourceType, id);

        var isTracked = await _apiClient.IsResourceTrackedAsync(metadata, cancellationToken);

        if (!isTracked)
        {
            await NotifyResourceCreatedAsync(resourceType, id, cancellationToken);
        }
    }

    public async Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequestAsync(resourceType, id, ResourceChangeOperation.Add, cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceDeletedAsync(
        AuditTrailResourceType resourceType, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequestAsync(resourceType, id, ResourceChangeOperation.Delete, cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequestAsync(resourceType, id,ResourceChangeOperation.Update, cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    private async Task<CommitRequest> CreateCommitRequestAsync(
        AuditTrailResourceType resourceType,
        Guid resourceId,
        ResourceChangeOperation operation,
        CancellationToken cancellationToken)
    {
        var author = GetAuthorInformation();

        using var data = operation is not ResourceChangeOperation.Delete
            ? await GetDataFromResourceAsync(resourceType, resourceId, cancellationToken)
            : null;

        var timeStamp = operation switch
        {
            ResourceChangeOperation.Add => data?.SystemInfo.CreatedAt,
            ResourceChangeOperation.Update => data?.SystemInfo.ModifiedAt,
            ResourceChangeOperation.Delete => DateTime.UtcNow,
            _ => throw new NotImplementedException(),
        };

        return new CommitRequest()
        {
            Author = author,
            TimeStamp = timeStamp,
            ResourceChanges = [
                  new()
                  {
                      Data = data?.GetDataInBytes(),
                      Operation = operation,
                      ResourceMetadata = GetResourceMetadata(resourceType, resourceId)
                  }
                  ],
        };
    }

    private static ResourceMetadata GetResourceMetadata(AuditTrailResourceType resourceType, Guid resourceId)
    {
        var resourceTypeName = resourceType.ToString();

        return new()
        {
            Id = resourceId,
            ResourceType = resourceTypeName,
            Filename = $"{resourceTypeName}_{resourceId}.json"
        };
    }

    private async Task<ResourceData> GetDataFromResourceAsync(
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
            AuditTrailResourceType.DatasetStructure => throw new NotImplementedException(),
            AuditTrailResourceType.MappingTableRelations => await TryGetMappingRelationsDataAsync(id, cancellationToken),
            _ => throw new NotImplementedException()
        } ?? throw new NotFoundException($"The resource of type '{resourceType}' and id {id}' was not found.");
    }

    private Author GetAuthorInformation()
    {
        var email = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType) ??
            throw new InvalidOperationException($"A valid user email must be provided.");

        var firstName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType);
        var lastName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType);

       return new()
        {
            Email = email,
            Name = $"{firstName} {lastName}"
        };
    }

    private async Task<ResourceData?> TryGetAgentDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetAgentAsync(id, cancellationToken);

        return resource is not null
            ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
            : null;
    }

    private async Task<ResourceData?> TryGetDcatCatalogDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDcatCatalogAsync(id, cancellationToken);

        return resource is not null
            ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
            : null;
    }

    private async Task<ResourceData?> TryGetDcatCatalogRecordsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var catalog = await _unrestrictedReaderService.TryGetDcatCatalogAsync(id, cancellationToken);
        var resource = await _unrestrictedReaderService.TryGetDcatCatalogRecordsAsync(id, cancellationToken);

        return catalog is not null
            ? new ResourceData(catalog.System, IopJsonSerializer.Serialize(resource))
            : null;
    }

    private async Task<ResourceData?> TryGetDataServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDataServiceAsync(id, cancellationToken);

        return resource is not null
             ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private async Task<ResourceData?> TryGetDatasetDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDatasetAsync(id, cancellationToken);

        return resource is not null
             ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private async Task<ResourceData?> TryGetConceptDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetIopConceptAsync(id, cancellationToken);

        return resource is not null
             ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private async Task<ResourceData?> TryGetCodeListEntriesDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var concept = await _unrestrictedReaderService.TryGetIopConceptAsync(id, cancellationToken);
        var resource = await _unrestrictedReaderService.TryGetCodeListEntriesAsync(id, cancellationToken);

        return concept is not null
             ? new ResourceData(concept.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private async Task<ResourceData?> TryGetMappingTableDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetMappingTableAsync(id, cancellationToken);

        return resource is not null
             ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private async Task<ResourceData?> TryGetMappingRelationsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var mappingTable = await _unrestrictedReaderService.TryGetMappingTableAsync(id, cancellationToken);
        var resource = await _unrestrictedReaderService.TryGetMappingRelationsAsync(id, cancellationToken);

        return mappingTable is not null
            ? new ResourceData(mappingTable.System, IopJsonSerializer.Serialize(resource))
            : null;
    }

    private async Task<ResourceData?> TryGetPublicServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetPublicServiceAsync(id, cancellationToken);

        return resource is not null
             ? new ResourceData(resource.System, IopJsonSerializer.Serialize(resource))
             : null;
    }

    private sealed record ResourceData(SystemInfoModel SystemInfo, Stream Data) : IDisposable
    {
        public void Dispose()
        {
            Data.Dispose();
        }

        public byte[] GetDataInBytes()
        {
            byte[] bytes = new byte[Data.Length];
            Data.ReadExactly(bytes);

            return bytes;
        }
    }
}
