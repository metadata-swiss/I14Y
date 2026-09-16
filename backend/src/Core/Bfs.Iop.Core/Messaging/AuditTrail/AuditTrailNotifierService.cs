using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Messaging.Queues;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed class AuditTrailNotifierService : IAuditTrailNotifierService
{
    private readonly IMessageQueue<AuditTrailMessage> _queue;
    private readonly ILogger<AuditTrailNotifierService> _logger;
    private readonly IUserContextService _userContextService;

    public AuditTrailNotifierService(
        IMessageQueue<AuditTrailMessage> queue,
        IUserContextService userContextService,
        ILogger<AuditTrailNotifierService> logger)
    {
        _queue = queue;
        _userContextService = userContextService;
        _logger = logger;
    }

    public Task NotifyFileCreated(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task NotifyFileDeleted(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task NotifyFileUpdated(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType, 
        IReadOnlyModel resource, 
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = CreateCommitRequest(resourceType, resource.Id, resource, ResourceChangeOperation.Add);

        var message = new AuditTrailMessage(commitRequest);

        return _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    public Task NotifyResourceDeletedAsync(
        AuditTrailResourceType resourceType, 
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = CreateCommitRequest(resourceType, id, default, ResourceChangeOperation.Delete);

        var message = new AuditTrailMessage(commitRequest);

        return _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    public Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType, 
        IReadOnlyModel resource,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = CreateCommitRequest(resourceType, resource.Id, resource, ResourceChangeOperation.Update);

        var message = new AuditTrailMessage(commitRequest);

        return _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    private CommitRequest CreateCommitRequest(
        AuditTrailResourceType resourceType,
        Guid resourceId, 
        IReadOnlyModel? resource,
        ResourceChangeOperation operation)
    {
        var email = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType) ??
            throw new InvalidOperationException($"A valid user email must be provided.");

        var firstName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType);
        var lastName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType);

        var author = new Author()
        {
            Email = email,
            Name = $"{firstName} {lastName}"
        };

        var data = resource is not null 
            ? SerializeResourceToJson(resource) 
            : null;

        var resourceTypeName = resourceType.ToString();

        return new CommitRequest()
        {
            Author = author,
            TimeStamp = DateTimeOffset.UtcNow,
            ResourceChanges = [
                  new()
                  {
                      Data = data,
                      Operation = operation,
                      ResourceMetadata = new()
                      {
                          Id = resourceId,
                          Name = resourceTypeName,
                          Filename = $"{resourceTypeName}_{resourceId}.json"
                      }
                  }
                  ],
        };
    }

    private static byte[] SerializeResourceToJson<T>(T resource)
    {
        var stream = resource switch
        {
            AgentModel model => IopJsonSerializer.Serialize(model),
            DcatCatalogModel model => IopJsonSerializer.Serialize(model),
            DcatDatasetModel model => IopJsonSerializer.Serialize(model),
            DataServiceModel model => IopJsonSerializer.Serialize(model),
            IopConceptModel model => IopJsonSerializer.Serialize(model),
            MappingTableModel model => IopJsonSerializer.Serialize(model),
            PublicServiceModel model => IopJsonSerializer.Serialize(model),
            _ => throw new NotSupportedException($"The type '{typeof(T)}' is not supported.")
        };

        byte[] bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);

        return bytes;
    }
}
