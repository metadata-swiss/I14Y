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

        var catalogs = await _subject.GetDcatCatalogIdsByDataset();

        catalogs[dataset].Should().BeEquivalentTo([first, second]);
    }

    [Test]
    public async Task A_data_service_record_is_not_read_as_a_dataset()
    {
        var catalog = SeedCatalog();
        var dataService = Guid.NewGuid();

        Record(catalog, dataService, DcatCatalogType.DataService);
        await _dbContext.SaveChangesAsync();

        (await _subject.GetDcatCatalogIdsByDataset()).Should().NotContainKey(dataService);
    }

    [Test]
    public async Task Asking_for_one_dataset_returns_only_that_dataset()
    {
        var catalog = SeedCatalog();
        var wanted = Guid.NewGuid();
        var other = Guid.NewGuid();

        Record(catalog, wanted, DcatCatalogType.Dataset);
        Record(catalog, other, DcatCatalogType.Dataset);
        await _dbContext.SaveChangesAsync();

        var catalogs = await _subject.GetDcatCatalogIdsByDataset(wanted);

        catalogs.Keys.Should().BeEquivalentTo([wanted]);
        catalogs[wanted].Should().BeEquivalentTo([catalog]);
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
