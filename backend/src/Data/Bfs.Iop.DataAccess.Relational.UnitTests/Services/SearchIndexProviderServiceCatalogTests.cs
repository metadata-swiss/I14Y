using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

[TestFixture(TestOf = typeof(SearchIndexProviderService))]
internal class SearchIndexProviderServiceCatalogTests
{
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
    public async Task Each_dataset_lists_every_catalogue_recording_it_once()
    {
        var dataset = Guid.NewGuid();
        var first = SeedCatalog();
        var second = SeedCatalog();

        Record(first, dataset, DcatCatalogType.Dataset);
        Record(first, dataset, DcatCatalogType.Dataset);
        Record(second, dataset, DcatCatalogType.Dataset);
        await _dbContext.SaveChangesAsync();

        var catalogs = await _subject.GetDcatCatalogIdsByResource();

        catalogs[dataset].Should().BeEquivalentTo([first, second]);
    }

    [Test]
    public async Task A_data_service_lists_the_catalogues_recording_it()
    {
        var catalog = SeedCatalog();
        var dataService = Guid.NewGuid();

        Record(catalog, dataService, DcatCatalogType.DataService);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetDcatCatalogIdsByResource())[dataService].Should().BeEquivalentTo([catalog]);
    }

    [Test]
    public async Task Asking_for_one_resource_returns_only_that_resource()
    {
        var catalog = SeedCatalog();
        var wanted = Guid.NewGuid();
        var other = Guid.NewGuid();

        Record(catalog, wanted, DcatCatalogType.Dataset);
        Record(catalog, other, DcatCatalogType.Dataset);
        await _dbContext.SaveChangesAsync();

        var catalogs = await _subject.GetDcatCatalogIdsByResource(wanted);

        catalogs.Keys.Should().BeEquivalentTo([wanted]);
        catalogs[wanted].Should().BeEquivalentTo([catalog]);
    }

    [Test]
    public async Task A_catalogue_lists_each_resource_it_records_once_with_its_type()
    {
        var catalog = SeedCatalog();
        var other = SeedCatalog();
        var dataset = Guid.NewGuid();
        var dataService = Guid.NewGuid();

        Record(catalog, dataset, DcatCatalogType.Dataset);
        Record(catalog, dataset, DcatCatalogType.Dataset);
        Record(catalog, dataService, DcatCatalogType.DataService);
        Record(other, Guid.NewGuid(), DcatCatalogType.Dataset);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetDcatCatalogResources(catalog)).Should().BeEquivalentTo(
        [
            new DcatCatalogResourceModel { ResourceId = dataset, ResourceType = DcatCatalogType.Dataset },
            new DcatCatalogResourceModel { ResourceId = dataService, ResourceType = DcatCatalogType.DataService },
        ]);
    }

    private Guid SeedCatalog()
    {
        var entity = EntitiesHelper.DcatCatalog;
        _dbContext.DcatCatalogs.Add(entity);
        return entity.Id;
    }

    private void Record(Guid catalogId, Guid resourceId, DcatCatalogType type)
    {
        var recordId = Guid.NewGuid();

        _dbContext.DcatCatalogRecords.Add(new DcatCatalogRecord
        {
            Id = recordId,
            DcatCatalogId = catalogId,
            PrimaryTopic = new DcatCatalogResource
            {
                Id = Guid.NewGuid(),
                DcatCatalogRecordId = recordId,
                ResourceId = resourceId,
                ResourceType = type.ToString(),
            },
        });
    }
}
