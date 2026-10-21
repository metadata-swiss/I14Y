using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.AuditTrail.ApiClient.Configuration;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed class AuditTrailNotifierService : IAuditTrailNotifierService
{
    private static readonly Author IosAuthor = new()
    {
        Email = "no-reply@i14y.ch",
        Name = "i14yIos",
    };

    private readonly IAuditTrailApiClient _apiClient;
    private readonly IMessageQueue<AuditTrailMessage> _queue;
    private readonly ILogger<AuditTrailNotifierService> _logger;
    private readonly IUserContextService _userContextService;
    private readonly IAgentsService _agentsService;
    private readonly IResourceStringReaderService _resourceStringReaderService;

    public AuditTrailNotifierService(
        IAuditTrailApiClient auditTrailApiClient,
        IMessageQueue<AuditTrailMessage> queue,
        IUserContextService userContextService,
        IAgentsService agentsService,
        IResourceStringReaderService resourceStringReaderService,
        ILogger<AuditTrailNotifierService> logger)
    {
        _apiClient = auditTrailApiClient;
        _queue = queue;
        _userContextService = userContextService;
        _agentsService = agentsService;
        _resourceStringReaderService = resourceStringReaderService;
        _logger = logger;
    }

    public async Task EnsureResourceIsTrackedAsync(
        AuditTrailResourceType resourceType, 
        Guid id, 
        CancellationToken cancellationToken)
    {
        var metadata = GetResourceMetadata(resourceType, id);

        if (await _apiClient.IsResourceTrackedAsync(metadata, cancellationToken))
        {
            return;
        }

        var commitRequest = await CreateCommitRequest(
            resourceType,
            id,
            ResourceChangeOperation.Add,
            data: await _resourceStringReaderService.GetResourceStringAsync(resourceType, id, cancellationToken),
            cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceCreatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequest(
            resourceType,
            id,
            ResourceChangeOperation.Add,
            data: null,
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

        var commitRequest = await CreateCommitRequest(resourceType, id, ResourceChangeOperation.Delete, data: null, cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    public async Task NotifyResourceUpdatedAsync(
        AuditTrailResourceType resourceType,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        resourceType.EnsureValueIsValid();

        var commitRequest = await CreateCommitRequest(
            resourceType,
            id,
            ResourceChangeOperation.Update,
            data: null,
            cancellationToken);

        var message = new AuditTrailMessage(commitRequest);

        await _queue.EnqueueAsync(message, cancellationToken);
    }

    private async Task<CommitRequest> CreateCommitRequest(
        AuditTrailResourceType resourceType,
        Guid resourceId,
        ResourceChangeOperation operation,
        string? data = null,
        CancellationToken cancellationToken = default)
    {
        return new CommitRequest()
        {
            Author = await GetAuthorAsync(cancellationToken),
            ResourceChanges = [
                  new()
                  { 
                      Data = data,
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

    private async Task<Author> GetAuthorAsync(CancellationToken cancellationToken = default)
    {
        var author = _userContextService.IsTechnicalUser()
            ? await GetAgentAuthorAsync(cancellationToken)
            : GetUserAuthor();

        return author;
    }

    private async Task<Author> GetAgentAuthorAsync(CancellationToken cancellationToken)
    {
        if (_userContextService.GetUserBusinessRole() is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return IosAuthor;
        }

        var agentIdentifiers = _userContextService.GetUserAgencies();

        var agents = await _agentsService.GetAgents(agentIdentifiers, cancellationToken);

        // Keep the order of the token claim so that the resolved author is deterministic.
        var agent = agentIdentifiers
            .Select(identifier => agents.FirstOrDefault(x => x.Identifier == identifier))
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.ContactPoint?.HasEmail));

        if (agent is null)
        {
            _logger.LogWarning(
                "No agent with a contact point email could be resolved for identifiers {AgentIdentifiers}.",
                string.Join(", ", agentIdentifiers));
        }

        return new()
        {
            Email = agent is null 
                ? $"no-reply@{agentIdentifiers[0]}.ch"
                : agent.ContactPoint!.HasEmail,
            Name = agentIdentifiers[0],
        };
    }

    private Author GetUserAuthor()
    {
        var email = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType) ??
            throw new InvalidOperationException($"A valid user email must be provided.");

        var firstName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType);
        var lastName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType);

       return new()
        {
            Email = email,
            Name = string.IsNullOrWhiteSpace($"{firstName} {lastName}") ? email : $"{firstName} {lastName}".Trim()
        };
    }
}
