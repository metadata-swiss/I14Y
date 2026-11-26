using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.Core.CommandHandlers.Agents;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Commands.Agents;

[TestFixture]
internal sealed class UpdateAgentSearchIndexNotificationTests
{
    private static readonly Guid AgentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private IAgentsService _agentsService = null!;
    private ISearchIndexNotifierService _searchIndex = null!;
    private IRequestHandler<UpdateAgentCommand> _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _agentsService = Substitute.For<IAgentsService>();
        _searchIndex = Substitute.For<ISearchIndexNotifierService>();

        _handler = new UpdateAgentCommandHandler(
            _agentsService,
            Substitute.For<IAuditTrailNotifierService>(),
            _searchIndex);
    }

    [Test]
    public async Task An_update_queues_one_agent_change_however_many_resources_reference_it()
    {
        await _handler.Handle(Update(), CancellationToken.None);

        await _searchIndex.Received(1).NotifyAgentChangedAsync(AgentId, Arg.Any<CancellationToken>());
        await _searchIndex.DidNotReceiveWithAnyArgs().NotifyResourceChangedAsync(default, default, default);
    }

    [Test]
    public async Task A_rejected_update_queues_nothing()
    {
        _agentsService.UpdateAgent(AgentId, Arg.Any<AgentInputModel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("validation failed")));

        var update = async () => await _handler.Handle(Update(), CancellationToken.None);

        await update.Should().ThrowAsync<InvalidOperationException>();
        await _searchIndex.DidNotReceiveWithAnyArgs().NotifyAgentChangedAsync(default, default);
    }

    private static UpdateAgentCommand Update() => new(AgentId, new AgentInputModel
    {
        ContactPoint = new VCardModel { HasEmail = "info@example.test" },
        Identifier = "CH_TEST",
        Name = new MultiLanguageModel { De = "Test" },
        PrefLabel = new MultiLanguageModel { De = "Test" },
    });
}
