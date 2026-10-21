using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

/// <summary>
///     The counts the catalogue index ranks on: how many public resources reference each resource,
///     worked out for no caller at all.
/// </summary>
[TestFixture(TestOf = typeof(SearchIndexProviderService))]
internal class SearchIndexProviderServiceReuseTests
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
    public async Task A_dataset_counts_the_public_data_services_serving_it()
    {
        var dataset = SeedDataset(PublicationLevel.Public);
        var served = SeedDataService(PublicationLevel.Public);
        var hidden = SeedDataService(PublicationLevel.Internal);

        Relate(served, dataset);
        Relate(hidden, dataset);
        await _dbContext.SaveChangesAsync();

        // The internal one is left out deliberately: the number is written into a document every
        // caller reads, so counting it would tell an anonymous visitor that it exists.
        (await Counts()).GetValueOrDefault(dataset).Should().Be(1);
    }

    [Test]
    public async Task A_data_service_counts_the_public_datasets_referencing_it()
    {
        var dataService = SeedDataService(PublicationLevel.Public);
        var referencing = SeedDataset(PublicationLevel.Public);
        var hidden = SeedDataset(PublicationLevel.Internal);

        Relate(dataService, referencing);
        Relate(dataService, hidden);
        await _dbContext.SaveChangesAsync();

        (await Counts()).GetValueOrDefault(dataService).Should().Be(1);
    }

    [Test]
    public async Task A_public_service_counts_what_points_at_it_rather_than_what_it_points_at()
    {
        var reused = SeedPublicService(PublicationLevel.Public);
        var linksToIt = SeedPublicService(PublicationLevel.Public);
        var requiresIt = SeedPublicService(PublicationLevel.Public);

        _dbContext.PublicServicesRelations.Add(new PublicServiceRelation
        {
            Id = Guid.NewGuid(),
            PublicServiceId = linksToIt,
            RelationId = reused,
        });

        _dbContext.Set<PublicServiceRequires>().Add(new PublicServiceRequires
        {
            Id = Guid.NewGuid(),
            PublicServiceId = requiresIt,
            RequiresId = reused,
        });

        await _dbContext.SaveChangesAsync();

        var counts = await Counts();

        // Both kinds of incoming reference are reuse, so they add up.
        counts.GetValueOrDefault(reused).Should().Be(2);

        // The services doing the referencing are not themselves reused by anyone.
        counts.GetValueOrDefault(linksToIt).Should().Be(0);
        counts.GetValueOrDefault(requiresIt).Should().Be(0);
    }

    [Test]
    public async Task A_dataset_counts_the_public_services_described_at_it()
    {
        var dataset = SeedDataset(PublicationLevel.Public);
        var describing = SeedPublicService(PublicationLevel.Public);
        var hidden = SeedPublicService(PublicationLevel.Internal);

        _dbContext.PublicServiceDescribedAtDatasetRelations.AddRange(
            new PublicServiceIsDescribedAt
            {
                Id = Guid.NewGuid(),
                PublicServiceId = describing,
                IsDescribedAtId = dataset,
            },
            new PublicServiceIsDescribedAt
            {
                Id = Guid.NewGuid(),
                PublicServiceId = hidden,
                IsDescribedAtId = dataset,
            });

        await _dbContext.SaveChangesAsync();

        (await Counts()).GetValueOrDefault(dataset).Should().Be(1);
    }

    [Test]
    public async Task The_standards_a_mapping_table_conforms_to_are_not_reuse_of_it()
    {
        var mappingTable = SeedMappingTable("urn:a", "urn:b", PublicationLevel.Public);

        foreach (var _ in Enumerable.Range(0, 3))
        {
            var resource = EntitiesHelper.Resource;
            resource.MappingTableConformsToId = mappingTable;
            _dbContext.Set<Resource>().Add(resource);
        }

        await _dbContext.SaveChangesAsync();

        (await Counts()).Should().NotContainKey(mappingTable);
    }

    [Test]
    public async Task A_concept_counts_the_public_mapping_tables_naming_it()
    {
        var concept = SeedConcept("c-1", "1.0.0");
        var other = SeedConcept("c-2", "1.0.0");

        SeedMappingTable(Iri("c-1", "1.0.0"), Iri("c-2", "1.0.0"), PublicationLevel.Public);
        SeedMappingTable(Iri("c-1", "1.0.0"), "urn:elsewhere", PublicationLevel.Internal);

        await _dbContext.SaveChangesAsync();

        var counts = await Counts();

        counts.GetValueOrDefault(concept).Should().Be(1);
        counts.GetValueOrDefault(other).Should().Be(1);
    }

    [Test]
    public async Task A_mapping_table_naming_one_concept_at_both_ends_counts_once()
    {
        var concept = SeedConcept("c-1", "1.0.0");

        SeedMappingTable(Iri("c-1", "1.0.0"), Iri("c-1", "1.0.0"), PublicationLevel.Public);
        await _dbContext.SaveChangesAsync();

        (await Counts()).GetValueOrDefault(concept).Should().Be(1);
    }

    [Test]
    public async Task A_concept_named_through_another_host_still_counts()
    {
        var concept = SeedConcept("c-1", "1.0.0");

        SeedMappingTable(
            "https://dev.example.ch/concept/c-1/version/1.0.0",
            "urn:elsewhere",
            PublicationLevel.Public);

        await _dbContext.SaveChangesAsync();

        // The identifier and version name the concept; the host the IRI was written with does not.
        (await Counts()).GetValueOrDefault(concept).Should().Be(1);
    }

    [Test]
    public async Task A_concept_whose_version_differs_is_a_different_concept()
    {
        var concept = SeedConcept("c-1", "1.0.0");

        SeedMappingTable(Iri("c-1", "2.0.0"), "urn:elsewhere", PublicationLevel.Public);
        await _dbContext.SaveChangesAsync();

        (await Counts()).GetValueOrDefault(concept).Should().Be(0);
    }

    [Test]
    public async Task A_concept_nothing_references_is_absent_rather_than_zero()
    {
        var concept = SeedConcept("c-1", "1.0.0");
        await _dbContext.SaveChangesAsync();

        // The rebuilder turns absence into a zero. Writing zeroes here instead would mean one row
        // per catalogue resource, almost all of them saying nothing.
        (await Counts()).Should().NotContainKey(concept);
    }

    [Test]
    public async Task A_concept_nothing_can_name_is_left_out()
    {
        SeedConcept("c-1", string.Empty);
        SeedConcept(string.Empty, "1.0.0");
        var complete = SeedConcept("c-3", "1.0.0");

        await _dbContext.SaveChangesAsync();

        (await _subject.GetConceptIdentities()).Select(x => x.Id)
            .Should().BeEquivalentTo([complete]);
    }

    [Test]
    public async Task Only_public_datasets_are_offered_for_filtering_references_read_elsewhere()
    {
        var published = SeedDataset(PublicationLevel.Public);
        var internalOnly = SeedDataset(PublicationLevel.Internal);

        await _dbContext.SaveChangesAsync();

        var ids = await _subject.GetPublicDatasetIds();

        ids.Should().Contain(published);
        ids.Should().NotContain(internalOnly);
    }

    /// <summary>The two halves share one read of the concepts, as the rebuild does.</summary>
    private async Task<IReadOnlyDictionary<Guid, int>> Counts() =>
        await _subject.GetPublicReuseCounts(await _subject.GetConceptIdentities());

    private static string Iri(string identifier, string version) =>
        $"https://example.org/i14y/concept/{identifier}/version/{version}";

    private void Relate(Guid dataServiceId, Guid datasetId) =>
        _dbContext.DataServiceDatasetRelations.Add(new DataServiceDataset
        {
            Id = Guid.NewGuid(),
            DataServiceId = dataServiceId,
            DatasetId = datasetId,
        });

    private Guid SeedDataService(PublicationLevel level)
    {
        var entity = EntitiesHelper.DataService;
        entity.PublicationLevel = level;
        _dbContext.DataServices.Add(entity);
        return entity.Id;
    }

    private Guid SeedDataset(PublicationLevel level)
    {
        var entity = EntitiesHelper.Dataset;
        entity.PublicationLevel = level;
        _dbContext.Datasets.Add(entity);
        return entity.Id;
    }

    private Guid SeedPublicService(PublicationLevel level)
    {
        var entity = EntitiesHelper.PublicService;
        entity.PublicationLevel = level;
        _dbContext.PublicServices.Add(entity);
        return entity.Id;
    }

    private Guid SeedConcept(string identifier, string version)
    {
        var entity = EntitiesHelper.IopConcept;
        entity.Id = Guid.NewGuid();
        entity.Identifiers = identifier.Length == 0 ? [] : [identifier];
        entity.Version = version;
        entity.ConformsTo = [];
        _dbContext.IopConcepts.Add(entity);
        return entity.Id;
    }

    private Guid SeedMappingTable(string sourceUri, string targetUri, PublicationLevel level)
    {
        var entity = EntitiesHelper.MappingTable;
        entity.Id = Guid.NewGuid();
        entity.SourceUri = sourceUri;
        entity.TargetUri = targetUri;
        entity.PublicationLevel = level;
        _dbContext.MappingTables.Add(entity);
        return entity.Id;
    }
}
