using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.AuditTrail.ApiClient.Configuration;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Messaging.AuditTrail;

[TestFixture(TestOf = typeof(AuditTrailNotifierService))]
internal sealed class AuditTrailNotifierServiceTests
{
    private IAuditTrailApiClient _apiClient = null!;
    private IMessageQueue<AuditTrailMessage> _queue = null!;
    private IUserContextService _userContextService = null!;
    private IAgentsService _agentsService = null!;
    private ILogger<AuditTrailNotifierService> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _apiClient = Substitute.For<IAuditTrailApiClient>();
        _queue = Substitute.For<IMessageQueue<AuditTrailMessage>>();
        _userContextService = Substitute.For<IUserContextService>();
        _agentsService = Substitute.For<IAgentsService>();
        _logger = Substitute.For<ILogger<AuditTrailNotifierService>>();

        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType)
            .Returns("user@example.com");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType)
            .Returns("Jane");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType)
            .Returns("Doe");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Given_create_or_update_notification_When_resource_exists_Then_request_is_enqueued(bool isCreate)
    {
        // Arrange
        var id = Guid.NewGuid();
        AuditTrailMessage? enqueuedMessage = null;

        _queue.EnqueueAsync(Arg.Any<AuditTrailMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                enqueuedMessage = callInfo.Arg<AuditTrailMessage>();
                return ValueTask.CompletedTask;
            });

        var service = CreateService();

        // Act
        if (isCreate)
        {
            await service.NotifyResourceCreatedAsync(AuditTrailResourceType.Agent, id, CancellationToken.None);
        }
        else
        {
            await service.NotifyResourceUpdatedAsync(AuditTrailResourceType.Agent, id, CancellationToken.None);
        }

        // Assert
        using var _ = new AssertionScope();
        enqueuedMessage.Should().NotBeNull();
        enqueuedMessage!.CommitRequest.ResourceChanges.Should().ContainSingle();

        var resourceChange = enqueuedMessage.CommitRequest.ResourceChanges.Single();
        resourceChange.ResourceMetadata.Id.Should().Be(id);
        resourceChange.ResourceMetadata.ResourceType.Should().Be(AuditTrailResourceType.Agent);
        resourceChange.Operation.Should().Be(
            isCreate
                ? ResourceChangeOperation.Add
                : ResourceChangeOperation.Update);
    }

    [Test]
    public async Task Given_user_token_When_notifying_Then_author_is_built_from_user_claims()
    {
        // Arrange
        var service = CreateService();

        // Act
        var author = await NotifyAndGetAuthorAsync(service);

        // Assert
        using var _ = new AssertionScope();
        author.Email.Should().Be("user@example.com");
        author.Name.Should().Be("Jane Doe");
        await _agentsService.DidNotReceiveWithAnyArgs().GetAgents(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Given_technical_token_When_notifying_Then_author_is_built_from_agent()
    {
        // Arrange
        SetupTechnicalClient("CH1");
        _agentsService.GetAgents(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([CreateAgent("CH1", "info@bfs.admin.ch", new() { En = "Federal Statistical Office (FSO)" })]);

        var service = CreateService();

        // Act
        var author = await NotifyAndGetAuthorAsync(service);

        // Assert
        using var _ = new AssertionScope();
        author.Email.Should().Be("info@bfs.admin.ch");
        author.Name.Should().Be("Federal Statistical Office (FSO)");
    }

    [Test]
    public async Task Given_technical_token_When_agent_cannot_be_resolved_Then_author_falls_back_to_user_claims()
    {
        // Arrange
        SetupTechnicalClient("CH1");
        _agentsService.GetAgents(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var service = CreateService();

        // Act
        var author = await NotifyAndGetAuthorAsync(service);

        // Assert
        using var _ = new AssertionScope();
        author.Email.Should().Be("user@example.com");
        author.Name.Should().Be("Jane Doe");
    }

    private void SetupTechnicalClient(params string[] agentIdentifiers)
    {
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.I14YClientTypeClaimType)
            .Returns(IopClaimsHelper.ClaimValues.I14YClientTypeClaimTechnicalValue);
        _userContextService.GetUserAgencies().Returns(agentIdentifiers);
    }

    private async Task<Author> NotifyAndGetAuthorAsync(AuditTrailNotifierService service)
    {
        AuditTrailMessage? enqueuedMessage = null;

        _queue.EnqueueAsync(Arg.Any<AuditTrailMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                enqueuedMessage = callInfo.Arg<AuditTrailMessage>();
                return ValueTask.CompletedTask;
            });

        await service.NotifyResourceCreatedAsync(AuditTrailResourceType.Agent, Guid.NewGuid(), CancellationToken.None);

        enqueuedMessage.Should().NotBeNull();

        return enqueuedMessage!.CommitRequest.Author;
    }

    private static AgentModel CreateAgent(string identifier, string email, MultiLanguageModel name) =>
        new()
        {
            Identifier = identifier,
            Name = name,
            PrefLabel = new(),
            System = new(),
            ContactPoint = new() { HasEmail = email },
        };

    private AuditTrailNotifierService CreateService() =>
        new(
            _apiClient,
            _queue,
            _userContextService,
            _agentsService,
            Options.Create(new AuditTrailOptions { BaseUrl = "https://audit-trail.example" }),
            _logger);
}