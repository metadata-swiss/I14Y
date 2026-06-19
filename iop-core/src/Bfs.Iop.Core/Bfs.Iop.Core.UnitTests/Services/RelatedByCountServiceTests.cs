using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Core.UnitTests.Helpers;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Services;

[TestFixture(TestOf = typeof(RelationsCountService))]
internal sealed class RelationsCountServiceTests
{
    private const string BaseIriUrl = "https://example.org/i14y";

    private IopDbContext _dbContext = null!;
    private IDatasetModelProcessService _datasetModelProcessService = null!;
    private RelationsCountService _subject = null!;

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

        _datasetModelProcessService = Substitute.For<IDatasetModelProcessService>();
        _datasetModelProcessService
            .GetConceptStructureReferencesBatch(Arg.Any<IEnumerable<IopConceptData>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, IReadOnlyList<Guid>>());

        _subject = new RelationsCountService(
            _dbContext,
            authorizationService,
            _datasetModelProcessService,
            Options.Create(new I14YOptions { IriBaseUrl = BaseIriUrl }));
    }

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Given_no_ids_When_GetDatasetServedByDataServiceCountBatch_Then_returns_empty()
    {
        var result = await _subject.GetDatasetServedByDataServiceCountBatch([]);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_dataset_with_public_data_services_When_GetDatasetServedByDataServiceCountBatch_Then_counts_distinct_data_services()
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
        var result = await _subject.GetDatasetServedByDataServiceCountBatch([datasetId]);

        // Assert
        result[datasetId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_data_service_When_GetDatasetServedByDataServiceCountBatch_Then_it_is_excluded_for_anonymous()
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
        var result = await _subject.GetDatasetServedByDataServiceCountBatch([datasetId]);

        // Assert — only the public data service is counted.
        result[datasetId].Should().Be(1);
    }

    [Test]
    public async Task Given_data_service_serving_public_datasets_When_GetDataServiceReferencedByDatasetCountBatch_Then_counts_distinct_datasets()
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
        var result = await _subject.GetDataServiceReferencedByDatasetCountBatch([dataServiceId]);

        // Assert — matches the detail page "Serves Dataset" section.
        result[dataServiceId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_served_dataset_When_GetDataServiceReferencedByDatasetCountBatch_Then_it_is_excluded_for_anonymous()
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
        var result = await _subject.GetDataServiceReferencedByDatasetCountBatch([dataServiceId]);

        // Assert — only the public served dataset is counted.
        result[dataServiceId].Should().Be(1);
    }

    [Test]
    public async Task Given_public_service_relating_to_and_requiring_services_When_GetPublicServiceReferencedByCountBatch_Then_sums_both_relations()
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
        var result = await _subject.GetPublicServiceReferencedByCountBatch([publicServiceId]);

        // Assert — 2 relation + 1 requires (the "Relation" + "Requires" detail sections).
        result[publicServiceId].Should().Be(3);
    }

    [Test]
    public async Task Given_internal_related_public_service_When_GetPublicServiceReferencedByCountBatch_Then_it_is_excluded_for_anonymous()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();

        _dbContext.PublicServicesRelations.AddRange(
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Public) },
            new PublicServiceRelation { Id = Guid.NewGuid(), PublicServiceId = publicServiceId, RelationId = SeedPublicService(PublicationLevel.Internal) });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetPublicServiceReferencedByCountBatch([publicServiceId]);

        // Assert — only the public related service counts.
        result[publicServiceId].Should().Be(1);
    }

    [Test]
    public async Task Given_public_service_describing_datasets_When_GetPublicServiceDescribesDatasetCountBatch_Then_counts_distinct_authorized_datasets()
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
        var result = await _subject.GetPublicServiceDescribesDatasetCountBatch([publicServiceId]);

        // Assert — matches the detail "Is described at" section; internal dataset excluded for anonymous.
        result[publicServiceId].Should().Be(2);
    }

    [Test]
    public async Task Given_mapping_table_with_conforming_resources_When_GetMappingTableReferencedByCountBatch_Then_counts_resources()
    {
        // Arrange — this relationship is intentionally not auth-filtered (resources have no publication level).
        var mappingTableId = Guid.NewGuid();

        _dbContext.Set<Resource>().AddRange(
            new Resource { Id = Guid.NewGuid(), Href = "https://a", MappingTableConformsToId = mappingTableId },
            new Resource { Id = Guid.NewGuid(), Href = "https://b", MappingTableConformsToId = mappingTableId });
        _dbContext.SaveChanges();

        // Act
        var result = await _subject.GetMappingTableReferencedByCountBatch([mappingTableId]);

        // Assert
        result[mappingTableId].Should().Be(2);
    }

    [Test]
    public async Task Given_public_mapping_table_matching_concept_on_both_ends_When_GetConceptMappingTableCountBatch_Then_counts_table_once()
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

        // Act
        var result = await _subject.GetConceptMappingTableCountBatch([conceptId]);

        // Assert
        result[conceptId].Should().Be(2);
    }

    [Test]
    public async Task Given_internal_mapping_table_matching_concept_When_GetConceptMappingTableCountBatch_Then_it_is_excluded_for_anonymous()
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

        // Act
        var result = await _subject.GetConceptMappingTableCountBatch([conceptId]);

        // Assert — only the public mapping table counts.
        result[conceptId].Should().Be(1);
    }

    [Test]
    public async Task Given_no_concepts_When_GetConceptMappingTableCountBatch_Then_returns_empty()
    {
        var result = await _subject.GetConceptMappingTableCountBatch([]);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_concept_structure_references_When_GetConceptStructureReferenceCountBatch_Then_counts_only_authorized_datasets()
    {
        // Arrange
        var conceptId = SeedConcept("concept-3", "1.0.0");
        var publicDatasetId = SeedDataset(PublicationLevel.Public);
        var internalDatasetId = SeedDataset(PublicationLevel.Internal);
        _dbContext.SaveChanges();

        // The triple store reports a structure-attribute reference in each dataset; only the
        // reference to the public dataset is visible to the anonymous caller.
        _datasetModelProcessService
            .GetConceptStructureReferencesBatch(Arg.Any<IEnumerable<IopConceptData>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, IReadOnlyList<Guid>>
            {
                [conceptId] = [publicDatasetId, internalDatasetId],
            });

        // Act
        var result = await _subject.GetConceptStructureReferenceCountBatch([conceptId]);

        // Assert — internal dataset's attribute excluded.
        result[conceptId].Should().Be(1);
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
