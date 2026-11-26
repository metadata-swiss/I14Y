using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

[TestFixture(TestOf = typeof(SearchIndexProviderService))]
internal sealed class SearchIndexProviderServiceAgentTests
{
    private static readonly Guid AgentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherAgentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private IopDbContext _dbContext = null!;
    private SearchIndexProviderService _subject = null!;

    [SetUp]
    public void Setup()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();

        _subject = new SearchIndexProviderService(
            _dbContext,
            Substitute.For<IVocabulariesService>());
    }

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Every_resource_the_agent_publishes_or_owns_is_returned_with_its_type()
    {
        var published = EntitiesHelper.DataService;
        published.TenantId = OtherAgentId;
        published.PublisherId = AgentId;

        var owned = EntitiesHelper.IopConcept;
        owned.TenantId = AgentId;

        var mappingTable = EntitiesHelper.MappingTable;
        mappingTable.TenantId = AgentId;

        var publicService = EntitiesHelper.PublicService;
        publicService.TenantId = AgentId;

        _dbContext.AddRange(published, owned, mappingTable, publicService);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetCatalogResourcesReferencingAgent(AgentId)).Should().BeEquivalentTo(
        [
            new CatalogResourceReferenceModel(published.Id, SearchResourceType.DataService),
            new CatalogResourceReferenceModel(owned.Id, SearchResourceType.Concept),
            new CatalogResourceReferenceModel(mappingTable.Id, SearchResourceType.MappingTable),
            new CatalogResourceReferenceModel(publicService.Id, SearchResourceType.PublicService),
        ]);
    }

    [Test]
    public async Task A_dataset_attributed_to_the_agent_is_returned()
    {
        var dataset = EntitiesHelper.Dataset;
        dataset.TenantId = OtherAgentId;
        dataset.QualifiedAttribution = [AttributionTo(AgentId)];

        _dbContext.Datasets.Add(dataset);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetCatalogResourcesReferencingAgent(AgentId)).Should().BeEquivalentTo(
            [new CatalogResourceReferenceModel(dataset.Id, SearchResourceType.Dataset)]);
    }

    [Test]
    public async Task A_dataset_referencing_the_agent_several_ways_is_returned_once()
    {
        var dataset = EntitiesHelper.Dataset;
        dataset.TenantId = AgentId;
        dataset.PublisherId = AgentId;
        dataset.QualifiedAttribution = [AttributionTo(AgentId), AttributionTo(AgentId)];

        _dbContext.Datasets.Add(dataset);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetCatalogResourcesReferencingAgent(AgentId)).Should().ContainSingle()
            .Which.Id.Should().Be(dataset.Id);
    }

    [Test]
    public async Task Resources_of_another_agent_are_not_returned()
    {
        var dataset = EntitiesHelper.Dataset;
        dataset.TenantId = OtherAgentId;
        dataset.PublisherId = OtherAgentId;
        dataset.QualifiedAttribution = [AttributionTo(OtherAgentId)];

        var dataService = EntitiesHelper.DataService;
        dataService.TenantId = OtherAgentId;

        _dbContext.AddRange(dataset, dataService);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetCatalogResourcesReferencingAgent(AgentId)).Should().BeEmpty();
    }

    private static QualifiedAttribution AttributionTo(Guid agentId)
    {
        var attribution = EntitiesHelper.QualifiedAttribution;
        attribution.AgentId = agentId;

        return attribution;
    }
}
