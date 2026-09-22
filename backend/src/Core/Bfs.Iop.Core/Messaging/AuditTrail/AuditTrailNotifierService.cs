using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.Common.Serialization.Json;
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
        const int maxAttempts = 10;
        const int delayInMs = 300;

        var metadata = GetResourceMetadata(resourceType, id);

        for (int i = 0; i < maxAttempts; i++)
        {
            if (await _apiClient.IsResourceTrackedAsync(metadata, cancellationToken))
            {
                return;
            }

            if (i == 1)
            {
                await NotifyResourceCreatedAsync(resourceType, id, cancellationToken);
            }

            await Task.Delay(delayInMs, cancellationToken);
        }

        throw new TimeoutException($"Resource {resourceType} with ID {id} could not be tracked before the operation.");
    }

    public async Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequestAsync(
            resourceType,
            id,
            ResourceChangeOperation.Add,
            cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceDeletedAsync(
        AuditTrailResourceType resourceType, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = CreateCommitRequest(resourceType, id, ResourceChangeOperation.Delete);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequestAsync(
            resourceType,
            id,
            ResourceChangeOperation.Update,
            cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    private async Task<CommitRequest> CreateCommitRequestAsync(
        AuditTrailResourceType resourceType,
        Guid resourceId,
        ResourceChangeOperation operation,
        CancellationToken cancellationToken)
    {
        var resourceData = await GetResourceDataAsync(resourceType, resourceId, cancellationToken);

        return CreateCommitRequest(resourceType, resourceId, operation, resourceData);
    }

    private CommitRequest CreateCommitRequest(
        AuditTrailResourceType resourceType,
        Guid resourceId,
        ResourceChangeOperation operation,
        string? resourceData = null)
    {
        var author = GetAuthorInformation();

        return new CommitRequest()
        {
            Author = author,
            ResourceChanges = [
                  new()
                  { 
                      Operation = operation,
                      ResourceMetadata = GetResourceMetadata(resourceType, resourceId),
                      ResourceData = resourceData
                  }
                  ],
        };
    }

    private async Task<string> GetResourceDataAsync(
       AuditTrailResourceType resourceType,
       Guid id,
       CancellationToken cancellationToken)
    {
        var resourceData = resourceType switch
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
        };

        return resourceData ?? throw new NotFoundException(
            $"The resource of type '{resourceType}' and id {id} was not found.");
    }

    private async Task<string?> TryGetAgentDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetAgentAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetDcatCatalogDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDcatCatalogAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetDcatCatalogRecordsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var catalog = await _unrestrictedReaderService.TryGetDcatCatalogAsync(id, cancellationToken);

        return catalog is null
            ? null
            : await SerializeAsync(await _unrestrictedReaderService.TryGetDcatCatalogRecordsAsync(id, cancellationToken));
    }

    private async Task<string?> TryGetDataServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDataServiceAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetDatasetDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetDatasetAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetConceptDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetIopConceptAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetCodeListEntriesDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var concept = await _unrestrictedReaderService.TryGetIopConceptAsync(id, cancellationToken);

        return concept is null
            ? null
            : await SerializeAsync(await _unrestrictedReaderService.TryGetCodeListEntriesAsync(id, cancellationToken));
    }

    private async Task<string?> TryGetMappingTableDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetMappingTableAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private async Task<string?> TryGetMappingRelationsDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var mappingTable = await _unrestrictedReaderService.TryGetMappingTableAsync(id, cancellationToken);

        return mappingTable is null
            ? null
            : await SerializeAsync(await _unrestrictedReaderService.TryGetMappingRelationsAsync(id, cancellationToken));
    }

    private async Task<string?> TryGetPublicServiceDataAsync(Guid id, CancellationToken cancellationToken)
    {
        var resource = await _unrestrictedReaderService.TryGetPublicServiceAsync(id, cancellationToken);

        return await TrySerializeAsync(resource);
    }

    private static async Task<string?> TrySerializeAsync<T>(T? resource)
        where T : class
    {
        return resource is null
            ? null
            : await SerializeAsync(resource);
    }

    private static async Task<string> SerializeAsync<T>(T resource)
        where T : class
    {
        await using var data = IopJsonSerializer.Serialize(resource);
        using var reader = new StreamReader(data);

        return await reader.ReadToEndAsync();
    }

    private static ResourceMetadata GetResourceMetadata(AuditTrailResourceType resourceType, Guid resourceId)
    {
        return new()
        {
            Id = resourceId,
            ResourceType = resourceType,
        };
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
}
