using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.CommandHandlers.Catalog;
using Bfs.Iop.Core.Services.Contracts;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Validation.Commands.Catalog;

[TestFixture(TestOf = typeof(GetRelationsCountCommandHandler))]
internal sealed class GetRelationsCountCommandHandlerTests
{
    private IRelationsCountService _relatedByCountService = null!;

    [SetUp]
    public void Setup()
    {
        _relatedByCountService = Substitute.For<IRelationsCountService>();

        // Default empty results so individual tests only set what they need.
        _relatedByCountService
            .GetConceptStructureReferenceCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetConceptMappingTableCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetDatasetServedByDataServiceCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetDataServiceReferencedByDatasetCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetPublicServiceReferencedByCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetPublicServiceDescribesDatasetCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
        _relatedByCountService
            .GetMappingTableReferencedByCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Empty);
    }

    [Test]
    public async Task Given_empty_request_When_Handle_Then_returns_empty()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetRelationsCountCommand([]), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task Given_concept_When_Handle_Then_total_sums_structure_and_mapping_counts()
    {
        // Arrange
        var conceptId = Guid.NewGuid();

        _relatedByCountService
            .GetConceptStructureReferenceCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [conceptId] = 3 });
        _relatedByCountService
            .GetConceptMappingTableCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [conceptId] = 2 });

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem
            {
                Id = conceptId,
                Type = SearchResourceType.Concept,
            },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert
        var model = result.Single();
        model.Id.Should().Be(conceptId);
        model.StructureAttribute.Should().Be(3);
        model.MappingTable.Should().Be(2);
        model.Total.Should().Be(5);
    }

    [Test]
    public async Task Given_dataset_When_Handle_Then_dataservice_count_is_total()
    {
        // Arrange
        var datasetId = Guid.NewGuid();

        _relatedByCountService
            .GetDatasetServedByDataServiceCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [datasetId] = 2 });

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem { Id = datasetId, Type = SearchResourceType.Dataset },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert — only "Is served by" data services are counted (matches the detail page).
        var model = result.Single();
        model.DataService.Should().Be(2);
        model.PublicService.Should().BeNull();
        model.Total.Should().Be(2);
    }

    [Test]
    public async Task Given_data_service_When_Handle_Then_dataset_count_is_total()
    {
        // Arrange
        var dataServiceId = Guid.NewGuid();

        _relatedByCountService
            .GetDataServiceReferencedByDatasetCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [dataServiceId] = 4 });

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem { Id = dataServiceId, Type = SearchResourceType.DataService },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert
        var model = result.Single();
        model.Dataset.Should().Be(4);
        model.Total.Should().Be(4);
    }

    [Test]
    public async Task Given_public_service_When_Handle_Then_total_sums_related_services_and_described_datasets()
    {
        // Arrange
        var publicServiceId = Guid.NewGuid();

        _relatedByCountService
            .GetPublicServiceReferencedByCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [publicServiceId] = 5 });
        _relatedByCountService
            .GetPublicServiceDescribesDatasetCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [publicServiceId] = 2 });

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem { Id = publicServiceId, Type = SearchResourceType.PublicService },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert — Is linked to + Requires → PublicService; Is described at → Dataset; Total = sum.
        var model = result.Single();
        model.PublicService.Should().Be(5);
        model.Dataset.Should().Be(2);
        model.Total.Should().Be(7);
    }

    [Test]
    public async Task Given_mapping_table_When_Handle_Then_only_total_is_populated()
    {
        // Arrange
        var mappingTableId = Guid.NewGuid();

        _relatedByCountService
            .GetMappingTableReferencedByCountBatch(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [mappingTableId] = 7 });

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem { Id = mappingTableId, Type = SearchResourceType.MappingTable },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert
        var model = result.Single();
        model.Total.Should().Be(7);
        model.StructureAttribute.Should().BeNull();
        model.MappingTable.Should().BeNull();
        model.DataService.Should().BeNull();
        model.PublicService.Should().BeNull();
        model.Dataset.Should().BeNull();
    }

    [Test]
    public async Task Given_mixed_batch_When_Handle_Then_returns_one_model_per_item_in_order()
    {
        // Arrange
        var conceptId = Guid.NewGuid();
        var datasetId = Guid.NewGuid();

        var handler = CreateHandler();

        var items = new[]
        {
            new RelationsCountRequestItem { Id = conceptId, Type = SearchResourceType.Concept },
            new RelationsCountRequestItem { Id = datasetId, Type = SearchResourceType.Dataset },
        };

        // Act
        var result = await handler.Handle(new GetRelationsCountCommand(items), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(conceptId);
        result[1].Id.Should().Be(datasetId);
    }

    private GetRelationsCountCommandHandler CreateHandler() => new(_relatedByCountService);

    private static IReadOnlyDictionary<Guid, int> Empty { get; } = new Dictionary<Guid, int>();
}
