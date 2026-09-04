using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Authorization;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

[TestFixture(TestOf = typeof(ResourceRelationsService))]
internal class ResourceRelationsServiceTests
{
    private const string BaseIriUrl = "https://example.org/i14y";

    private IopDbContext _dbContext = null!;
    private ResourceRelationsService _subject = null!;

    [SetUp]
    public void Setup()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();

        // Anonymous user context → read-auth allows only PublicationLevel.Public, so the counts
        // exclude Internal/private referencing entities (the leak the change addresses).
        var authorizationService = new PublishableEntityAuthorizationService(
            TestHelper.CreateFakeUserContextService([]),
            Substitute.For<IPublicationLevelPolicyService>(),
            Substitute.For<IRegistrationStatusPolicyService>());

        _subject = new ResourceRelationsService(
            _dbContext,
            authorizationService);
    }

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Given_no_ids_When_GetDatasetsServedByDataServicesCount_Then_returns_empty()
    {
        var result = await _subject.GetDatasetsServedByDataServicesCount([]);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_dataset_with_public_data_services_When_GetDatasetsServedByDataServicesCount_Then_counts_distinct_data_services()
    {
        // Arrange
        var datasetId = Guid.NewGuid();
        var dataServiceA = SeedDataService(PublicationLevel.Public);
        var dataServiceB = SeedDataService(PublicationLevel.Public);

        _dbContext.DataServiceDatasetRelations.AddRange(
            new DataServiceDataset { Id = Guid.NewGuid(), DatasetId = datasetId, DataServiceId = dataServiceA },
            new DataServiceDataset { Id = Guid.NewGuid(), DatasetId = datasetId, DataServiceId = dataServiceB });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetDatasetsServedByDataServicesCount([datasetId]);

        // Assert
        result[datasetId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_data_service_When_GetDatasetsServedByDataServicesCount_Then_it_is_excluded_for_anonymous()
    {
        // Arrange
        var datasetId = Guid.NewGuid();
        var publicDataService = SeedDataService(PublicationLevel.Public);
        var internalDataService = SeedDataService(PublicationLevel.Internal);

        _dbContext.DataServiceDatasetRelations.AddRange(
            new DataServiceDataset { Id = Guid.NewGuid(), DatasetId = datasetId, DataServiceId = publicDataService },
            new DataServiceDataset { Id = Guid.NewGuid(), DatasetId = datasetId, DataServiceId = internalDataService });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetDatasetsServedByDataServicesCount([datasetId]);

        // Assert — only the public data service is counted.
        result[datasetId].Should().Be(1);
    }

    [Test]
    public async Task Given_data_service_serving_public_datasets_When_GetDataServicesReferencedByDatasetsCount_Then_counts_distinct_datasets()
    {
        // Arrange
        var dataServiceId = Guid.NewGuid();
        var datasetA = SeedDataset(PublicationLevel.Public);
        var datasetB = SeedDataset(PublicationLevel.Public);

        _dbContext.DataServiceDatasetRelations.AddRange(
            new DataServiceDataset { Id = Guid.NewGuid(), DataServiceId = dataServiceId, DatasetId = datasetA },
            new DataServiceDataset { Id = Guid.NewGuid(), DataServiceId = dataServiceId, DatasetId = datasetB });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetDataServicesReferencedByDatasetsCount([dataServiceId]);

        // Assert — matches the detail page "Serves Dataset" section.
        result[dataServiceId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_served_dataset_When_GetDataServicesReferencedByDatasetsCount_Then_it_is_excluded_for_anonymous()
    {
        // Arrange
        var dataServiceId = Guid.NewGuid();
        var publicDatasetId = SeedDataset(PublicationLevel.Public);
        var internalDatasetId = SeedDataset(PublicationLevel.Internal);

        _dbContext.DataServiceDatasetRelations.AddRange(
            new DataServiceDataset { Id = Guid.NewGuid(), DataServiceId = dataServiceId, DatasetId = publicDatasetId },
            new DataServiceDataset { Id = Guid.NewGuid(), DataServiceId = dataServiceId, DatasetId = internalDatasetId });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetDataServicesReferencedByDatasetsCount([dataServiceId]);

        // Assert — only the public served dataset is counted.
        result[dataServiceId].Should().Be(1);
    }

    [Test]
    public async Task Given_public_service_relating_to_and_requiring_services_When_GetPublicServicesReferencedInRelationsCount_Then_return_relations()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();

        _dbContext.PublicServicesRelations.AddRange(
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Public) },
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Public) });

        _dbContext.Set<PublicServiceRequires>().Add(
            new PublicServiceRequires { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RequiresId = SeedPublicService(PublicationLevel.Public) });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetPublicServicesReferencedInRelationsCount([publicServiceId]);

        // Assert
        result[publicServiceId].Should().Be(2);
    }

    [Test]
    public async Task Given_public_service_relating_to_and_requiring_services_When_GetPublicServicesReferencedInRequiresCount_Then_return_relations()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();

        _dbContext.Set<PublicServiceRequires>().Add(
            new PublicServiceRequires { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RequiresId = SeedPublicService(PublicationLevel.Public) });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetPublicServicesReferencedInRequiresCount([publicServiceId]);

        // Assert
        result[publicServiceId].Should().Be(1);
    }


    [Test]
    public async Task Given_internal_related_public_service_When_GetPublicServicesReferencedInRelationsCount_Then_it_is_excluded_for_anonymous()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();

        _dbContext.PublicServicesRelations.AddRange(
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Public) },
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Internal) });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetPublicServicesReferencedInRelationsCount([publicServiceId]);

        // Assert — only the public related service counts.
        result[publicServiceId].Should().Be(1);
    }

    [Test]
    public async Task Given_public_service_describing_datasets_When_GetPublicServicesDescribedAtDatasetsCount_Then_counts_distinct_authorized_datasets()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();
        var datasetA = SeedDataset(PublicationLevel.Public);
        var datasetB = SeedDataset(PublicationLevel.Public);
        var internalDataset = SeedDataset(PublicationLevel.Internal);

        _dbContext.PublicServiceDescribedAtDatasetRelations.AddRange(
            new PublicServiceIsDescribedAt { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, IsDescribedAtId = datasetA },
            new PublicServiceIsDescribedAt { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, IsDescribedAtId = datasetB },
            new PublicServiceIsDescribedAt { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, IsDescribedAtId = internalDataset });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetPublicServicesDescribedAtDatasetsCount([publicServiceId]);

        // Assert — matches the detail "Is described at" section; internal dataset excluded for anonymous.
        result[publicServiceId].Should().Be(2);
    }

    [Test]
    public async Task Given_mapping_table_with_conforming_resources_When_GetMappingTablesReferencedInConformsToCount_Then_counts_resources()
    {
        // Arrange — this relationship is intentionally not auth-filtered (resources have no publication level).
        var mappingTableId = Guid.NewGuid();

        _dbContext.Set<Resource>().AddRange(
            new Resource { Id = Guid.NewGuid(), Href = "https://a", MappingTableConformsToId = mappingTableId },
            new Resource { Id = Guid.NewGuid(), Href = "https://b", MappingTableConformsToId = mappingTableId });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetMappingTablesReferencedInConformsToCount([mappingTableId]);

        // Assert
        result[mappingTableId].Should().Be(2);
    }

    [Test]
    public async Task Given_public_mapping_table_matching_concept_on_both_ends_When_GetConceptsReferencedInMappingTablesCount_Then_counts_table_once()
    {
        // Arrange
        const string identifier = "concept-1";
        const string version = "1.0.0";
        var conceptIri = $"{BaseIriUrl}/concept/{identifier}/version/{version}";

        // One table referencing the concept on BOTH source and target → must count once.
        // A second table referencing it only on the source → counts once.
        var conceptId = SeedConcept(identifier, version);
        _dbContext.MappingTables.AddRange(
            CreateMappingTable(conceptIri, conceptIri, PublicationLevel.Public),
            CreateMappingTable(conceptIri, "https://other", PublicationLevel.Public));
        _dbContext.SaveChanges();

        var dic = new Dictionary<string, Guid>
        {
            { conceptIri, conceptId }
        };

        // Act
        var result = await _subject.GetConceptsReferencedInMappingTablesCount(dic);

        // Assert
        result[conceptId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_mapping_table_matching_concept_When_GetConceptsReferencedInMappingTablesCount_Then_it_is_excluded_for_anonymous()
    {
        // Arrange
        const string identifier = "concept-2";
        const string version = "1.0.0";
        var conceptIri = $"{BaseIriUrl}/concept/{identifier}/version/{version}";

        var conceptId = SeedConcept(identifier, version);
        _dbContext.MappingTables.AddRange(
            CreateMappingTable(conceptIri, "https://other", PublicationLevel.Public),
            CreateMappingTable(conceptIri, "https://other2", PublicationLevel.Internal));
        _dbContext.SaveChanges();

        var dic = new Dictionary<string, Guid>
        {
            { conceptIri, conceptId }
        };

        // Act
        var result = await _subject.GetConceptsReferencedInMappingTablesCount(dic);

        // Assert — only the public mapping table counts.
        result[conceptId].Should().Be(1);
    }

    [Test]
    public async Task Given_no_concepts_When_GetConceptsReferencedInMappingTablesCount_Then_returns_empty()
    {
        var dic = new Dictionary<string, Guid>();

        var result = await _subject.GetConceptsReferencedInMappingTablesCount(dic);

        result.Should().BeEmpty();
    }

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
        entity.Identifiers = [identifier];
        entity.Version = version;
        _dbContext.IopConcepts.Add(entity);
        return entity.Id;
    }

    private static MappingTable CreateMappingTable(string sourceUri, string targetUri, PublicationLevel level)
    {
        var mappingTable = EntitiesHelper.MappingTable;
        mappingTable.Id = Guid.NewGuid();
        mappingTable.SourceUri = sourceUri;
        mappingTable.TargetUri = targetUri;
        mappingTable.PublicationLevel = level;
        return mappingTable;
    }
}
