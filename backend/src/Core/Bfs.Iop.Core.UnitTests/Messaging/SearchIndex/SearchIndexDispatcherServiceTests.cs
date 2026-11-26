using AwesomeAssertions;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Messaging.SearchIndex;

[TestFixture(TestOf = typeof(SearchIndexDispatcherService))]
internal sealed class SearchIndexDispatcherServiceTests
{
    private static readonly Guid AgentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(10);

    private IIndexSearchApiClient _client = null!;
    private ISearchIndexProviderService _provider = null!;
    private ChannelMessageQueue<SearchIndexMessage> _queue = null!;
    private ServiceProvider _services = null!;
    private SearchIndexDispatcherService _dispatcher = null!;

    [SetUp]
    public async Task SetUp()
    {
        _client = Substitute.For<IIndexSearchApiClient>();
        _provider = Substitute.For<ISearchIndexProviderService>();
        _queue = new ChannelMessageQueue<SearchIndexMessage>();

        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_provider)
            .BuildServiceProvider();

        _dispatcher = new SearchIndexDispatcherService(
            _queue,
            _services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<SearchIndexDispatcherService>.Instance);

        await _dispatcher.StartAsync(CancellationToken.None);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dispatcher.StopAsync(CancellationToken.None);
        _dispatcher.Dispose();
        await _services.DisposeAsync();
    }

    [Test]
    public async Task An_agent_change_reindexes_every_resource_referencing_the_agent()
    {
        var dataset = Guid.NewGuid();
        var concept = Guid.NewGuid();
        ReferencingAgent(
            new CatalogResourceReferenceModel(dataset, SearchResourceType.Dataset),
            new CatalogResourceReferenceModel(concept, SearchResourceType.Concept));

        await _queue.EnqueueAsync(AgentChange());
        await PutsReceived(2);

        await _client.Received(1).PutIndexCatalogByTypeAndIdAsync(SearchResourceType.Dataset, dataset, Arg.Any<CancellationToken>());
        await _client.Received(1).PutIndexCatalogByTypeAndIdAsync(SearchResourceType.Concept, concept, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task An_agent_referenced_by_more_resources_than_the_queue_holds_reindexes_them_all()
    {
        var resources = Enumerable.Range(0, 1500)
            .Select(_ => new CatalogResourceReferenceModel(Guid.NewGuid(), SearchResourceType.Dataset))
            .ToArray();
        ReferencingAgent(resources);

        await _queue.EnqueueAsync(AgentChange());
        await PutsReceived(resources.Length);
    }

    [Test]
    public async Task A_resource_the_search_service_rejects_does_not_stop_the_others()
    {
        var rejected = Guid.NewGuid();
        var accepted = Guid.NewGuid();
        ReferencingAgent(
            new CatalogResourceReferenceModel(rejected, SearchResourceType.Dataset),
            new CatalogResourceReferenceModel(accepted, SearchResourceType.Dataset));

        _client.PutIndexCatalogByTypeAndIdAsync(SearchResourceType.Dataset, rejected, Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<SwaggerResponse>(
                new ApiException("bad request", 400, string.Empty, new Dictionary<string, IEnumerable<string>>(), null!)));

        await _queue.EnqueueAsync(AgentChange());
        await PutsReceived(2);

        await _client.Received(1).PutIndexCatalogByTypeAndIdAsync(SearchResourceType.Dataset, accepted, Arg.Any<CancellationToken>());
    }

    private static SearchIndexMessage AgentChange() =>
        new(SearchIndexTarget.CatalogResourcesReferencingAgent, AgentId, SearchIndexOperation.Upsert);

    private void ReferencingAgent(params CatalogResourceReferenceModel[] resources) =>
        _provider.GetCatalogResourcesReferencingAgent(AgentId, Arg.Any<CancellationToken>()).Returns(resources);

    private async Task PutsReceived(int expected)
    {
        using var timeout = new CancellationTokenSource(DeliveryTimeout);

        while (PutCount() < expected && !timeout.IsCancellationRequested)
        {
            await Task.Delay(10, CancellationToken.None);
        }

        PutCount().Should().Be(expected);
    }

    private int PutCount() => _client.ReceivedCalls()
        .Count(call => call.GetMethodInfo().Name == nameof(IIndexSearchApiClient.PutIndexCatalogByTypeAndIdAsync));
}
