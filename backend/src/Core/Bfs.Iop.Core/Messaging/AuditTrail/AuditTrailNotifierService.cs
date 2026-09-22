using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Messaging;
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

        var commitRequest = CreateCommitRequest(resourceType, id, ResourceChangeOperation.Add);

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

        var commitRequest = CreateCommitRequest(resourceType, id,ResourceChangeOperation.Update);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken).AsTask();
    }

    private CommitRequest CreateCommitRequest(
        AuditTrailResourceType resourceType,
        Guid resourceId,
        ResourceChangeOperation operation)
    {
        var author = GetAuthorInformation();

        return new CommitRequest()
        {
            Author = author,
            ResourceChanges = [
                  new()
                  { 
                      Operation = operation,
                      ResourceMetadata = GetResourceMetadata(resourceType, resourceId)
                  }
                  ],
        };
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
