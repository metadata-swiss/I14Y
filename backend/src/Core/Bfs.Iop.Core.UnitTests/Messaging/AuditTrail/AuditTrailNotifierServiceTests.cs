using AwesomeAssertions;
using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Messaging.AuditTrail;

[TestFixture(TestOf = typeof(AuditTrailNotifierService))]
internal sealed class AuditTrailNotifierServiceTests
{
    private IAuditTrailApiClient _apiClient = null!;
    private IMessageQueue<AuditTrailMessage> _queue = null!;
    private IUserContextService _userContextService = null!;
    private IUnrestrictedReaderService _unrestrictedReaderService = null!;
    private ILogger<AuditTrailNotifierService> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _apiClient = Substitute.For<IAuditTrailApiClient>();
        _queue = Substitute.For<IMessageQueue<AuditTrailMessage>>();
        _userContextService = Substitute.For<IUserContextService>();
        _unrestrictedReaderService = Substitute.For<IUnrestrictedReaderService>();
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
    public async Task Given_create_or_update_notification_When_resource_exists_Then_snapshot_is_enqueued(bool isCreate)
    {
        // Arrange
        var id = Guid.NewGuid();
        var agent = ModelsHelper.AgentModel;
        var expectedResourceData = await SerializeAsync(agent);
        AuditTrailMessage? enqueuedMessage = null;

        _unrestrictedReaderService.TryGetAgentAsync(id, Arg.Any<CancellationToken>())
            .Returns(agent);

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
        enqueuedMessage.Should().NotBeNull();
        enqueuedMessage!.CommitRequest.ResourceChanges.Should().ContainSingle();

        var resourceChange = enqueuedMessage.CommitRequest.ResourceChanges.Single();
        resourceChange.ResourceMetadata.Id.Should().Be(id);
        resourceChange.ResourceMetadata.ResourceType.Should().Be(AuditTrailResourceType.Agent);
        resourceChange.Operation.Should().Be(
            isCreate
                ? ResourceChangeOperation.Add
                : ResourceChangeOperation.Update);
        resourceChange.ResourceData.Should().Be(expectedResourceData);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Given_create_or_update_notification_When_resource_is_missing_Then_it_throws_and_does_not_enqueue(bool isCreate)
    {
        // Arrange
        var id = Guid.NewGuid();
        var service = CreateService();

        // Act
        Func<Task> act = async () =>
        {
            if (isCreate)
            {
                await service.NotifyResourceCreatedAsync(AuditTrailResourceType.Agent, id, CancellationToken.None);
            }
            else
            {
                await service.NotifyResourceUpdatedAsync(AuditTrailResourceType.Agent, id, CancellationToken.None);
            }
        };

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _queue.DidNotReceive().EnqueueAsync(Arg.Any<AuditTrailMessage>(), Arg.Any<CancellationToken>());
    }

    private AuditTrailNotifierService CreateService() =>
        new(
            _apiClient,
            _queue,
            _userContextService,
            _unrestrictedReaderService,
            _logger);

    private static async Task<string> SerializeAsync<T>(T data)
        where T : class
    {
        await using var stream = IopJsonSerializer.Serialize(data);
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }
}
