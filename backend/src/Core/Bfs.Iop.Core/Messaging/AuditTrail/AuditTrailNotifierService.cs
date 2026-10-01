using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.AuditTrail.ApiClient.Configuration;
using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Messaging.AuditTrail;

internal sealed class AuditTrailNotifierService : IAuditTrailNotifierService
{
    private readonly IAuditTrailApiClient _apiClient;
    private readonly IMessageQueue<AuditTrailMessage> _queue;
    private readonly ILogger<AuditTrailNotifierService> _logger;
    private readonly IUserContextService _userContextService;
    private readonly IAgentsService _agentsService;
    private readonly AuditTrailOptions _options;

    public AuditTrailNotifierService(
        IAuditTrailApiClient auditTrailApiClient,
        IMessageQueue<AuditTrailMessage> queue,
        IUserContextService userContextService,
        IAgentsService agentsService,
        IOptions<AuditTrailOptions> options,
        ILogger<AuditTrailNotifierService> logger)
    {
        _apiClient = auditTrailApiClient;
        _queue = queue;
        _userContextService = userContextService;
        _agentsService = agentsService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureResourceIsTrackedAsync(
        AuditTrailResourceType resourceType, 
        Guid id, 
        CancellationToken cancellationToken)
    {
        var maxAttempts = _options.EnsureResourceIsTrackedMaxAttempts;
        var delayInMs = _options.EnsureResourceIsTrackedDelayInMs;

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
        var author = await GetAuthorInformationAsync(cancellationToken);

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

    private async Task<Author> GetAuthorInformationAsync(CancellationToken cancellationToken)
    {
        var agentAuthor = IsTechnicalClient()
            ? await TryGetAgentAuthorAsync(cancellationToken)
            : null;

        return agentAuthor ?? GetUserAuthor();
    }

    private bool IsTechnicalClient() =>
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.I14YClientTypeClaimType)
            is IopClaimsHelper.ClaimValues.I14YClientTypeClaimTechnicalValue;

    private async Task<Author?> TryGetAgentAuthorAsync(CancellationToken cancellationToken)
    {
        var agentIdentifiers = _userContextService.GetUserAgencies()
            .Where(x => !string.IsNullOrWhiteSpace(x) && x != "*")
            .ToList();

        if (agentIdentifiers.Count == 0)
        {
            _logger.LogWarning("Technical client token does not contain any agent identifier usable as audit trail author.");
            return null;
        }

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
            return null;
        }

        return new()
        {
            Email = agent.ContactPoint!.HasEmail,
            Name = GetAgentName(agent),
        };
    }

    private static string GetAgentName(AgentModel agent) =>
        new[] { agent.Name.En, agent.Name.De, agent.Name.Fr, agent.Name.It }
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? agent.Identifier;

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
