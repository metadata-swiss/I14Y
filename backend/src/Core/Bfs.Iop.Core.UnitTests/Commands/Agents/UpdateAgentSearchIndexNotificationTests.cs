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
    private ISearchIndexProviderService _searchIndexProvider = null!;
    private IRequestHandler<UpdateAgentCommand> _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _agentsService = Substitute.For<IAgentsService>();
        _searchIndex = Substitute.For<ISearchIndexNotifierService>();
        _searchIndexProvider = Substitute.For<ISearchIndexProviderService>();

        _searchIndexProvider.GetCatalogResourcesReferencingAgent(AgentId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<CatalogResourceReferenceModel>());

        _handler = new UpdateAgentCommandHandler(
            _agentsService,
            Substitute.For<IAuditTrailNotifierService>(),
            _searchIndex,
            _searchIndexProvider);
    }

    [Test]
    public async Task Every_resource_copying_the_agent_is_reindexed_with_its_type()
    {
        var dataset = Guid.NewGuid();
        var concept = Guid.NewGuid();

        _searchIndexProvider.GetCatalogResourcesReferencingAgent(AgentId, Arg.Any<CancellationToken>())
            .Returns(
            [
                new CatalogResourceReferenceModel(dataset, SearchResourceType.Dataset),
                new CatalogResourceReferenceModel(concept, SearchResourceType.Concept),
            ]);

        await _handler.Handle(Update(), CancellationToken.None);

        await _searchIndex.Received(1).NotifyResourceChangedAsync(SearchResourceType.Dataset, dataset, Arg.Any<CancellationToken>());
        await _searchIndex.Received(1).NotifyResourceChangedAsync(SearchResourceType.Concept, concept, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task An_agent_nothing_references_reindexes_nothing()
    {
        await _handler.Handle(Update(), CancellationToken.None);

        await _searchIndex.DidNotReceiveWithAnyArgs().NotifyResourceChangedAsync(default, default, default);
    }

    [Test]
    public async Task A_rejected_update_reindexes_nothing()
    {
        _agentsService.UpdateAgent(AgentId, Arg.Any<AgentInputModel>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("validation failed")));

        var update = async () => await _handler.Handle(Update(), CancellationToken.None);

        await update.Should().ThrowAsync<InvalidOperationException>();
        await _searchIndex.DidNotReceiveWithAnyArgs().NotifyResourceChangedAsync(default, default, default);
    }

    private static UpdateAgentCommand Update() => new(AgentId, new AgentInputModel
    {
        ContactPoint = new VCardModel { HasEmail = "info@example.test" },
        Identifier = "CH_TEST",
        Name = new MultiLanguageModel { De = "Test" },
        PrefLabel = new MultiLanguageModel { De = "Test" },
    });
}
