using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.CommandHandlers.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Core.UnitTests.Helpers;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.Logging;
using NSubstitute;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.UnitTests.Validation.Commands.DcatCatalogs;

[TestFixture(TestOf = typeof(ExportDcatCatalogCommandHandler))]
internal class ExportDcatCatalogCommandHandlerTests
{
    private IDcatCatalogsService _catalogService = null!;
    private IDataServicesService _dataServicesService = null!;
    private IDatasetsService _datasetsService = null!;
    private ILogger<ExportDcatCatalogCommandHandler> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _catalogService = Substitute.For<IDcatCatalogsService>();
        _dataServicesService = Substitute.For<IDataServicesService>();
        _datasetsService = Substitute.For<IDatasetsService>();
        _logger = Substitute.For<ILogger<ExportDcatCatalogCommandHandler>>();
    }

    [Test]
    public async Task Given_catalogue_with_dataset_and_distribution_When_exporting_to_dcat_rdf_Then_distribution_is_typed_as_dcat_distribution()
    {
        // Arrange

        _datasetsService.GetDataset(Arg.Any<Guid>()).Returns(ModelsHelper.DcatDatasetModel);

        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);

        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<DcatCatalogRecordModel>()
            {
                Page = 1,
                PageSize = 1,
                Results = [ModelsHelper.DcatCatalogRecordModelDataset],
                TotalCount = 1
            });

        var handler = CreateHandler();
        var command = new ExportDcatCatalogCommand(Guid.NewGuid(), RdfExportFormat.RDF);

        // Act
        var rdfXml = await handler.Handle(command, CancellationToken.None);

        // Assert
        using (var graph = new Graph())
        {
            var parser = new RdfXmlParser();
            using var reader = new StringReader(rdfXml);
            parser.Load(graph, reader);

            var dcatDistributionPredicate = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#distribution"));

            var rdfTypePredicate = graph.CreateUriNode(
                UriFactory.Create(RdfSpecsHelper.RdfType));

            var dcatDistributionType = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#Distribution"));

            var datasetToDistributionTriples =
                graph.GetTriplesWithPredicate(dcatDistributionPredicate).ToList();

            datasetToDistributionTriples.Should().NotBeEmpty(
                "the dataset should expose at least one distribution");

            foreach (var distributionNode in datasetToDistributionTriples.Select(t => t.Object))
            {
                graph.ContainsTriple(new Triple(
                    distributionNode,
                    rdfTypePredicate,
                    dcatDistributionType
                )).Should().BeTrue(
                    "each distribution must have rdf:type dcat:Distribution");
            }
        }
    }

    [Test]
    public async Task Given_catalogue_with_dataset_When_exporting_to_dcat_rdf_Then_dataset_URI_should_be_correct()
    {
        // Arrange

        _datasetsService.GetDataset(Arg.Any<Guid>()).Returns(ModelsHelper.DcatDatasetModel);

        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);

        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
              .Returns(new PagedResult<DcatCatalogRecordModel>()
              {
                  Page = 1,
                  PageSize = 1,
                  Results = [ModelsHelper.DcatCatalogRecordModelDataset],
                  TotalCount = 1
              });

        var handler = CreateHandler();
        var command = new ExportDcatCatalogCommand(Guid.NewGuid(), RdfExportFormat.RDF);

        // Act
        var rdfXml = await handler.Handle(command, CancellationToken.None);

        // Assert
        using (var graph = new Graph())
        {
            var parser = new RdfXmlParser();
            using var reader = new StringReader(rdfXml);
            parser.Load(graph, reader);

            var rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

            var dcatDatasetPredicate = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#dataset"));

            var dcatDatasetType = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#Dataset"));

            var identifier = ModelsHelper.DcatDatasetModel.Identifiers.First();

            var expectedDatasetUri = UriFactory.Create(
                $"https://iri.i14y.d.c.bfs.admin.ch/dataset/{identifier}");

            // 1) Catalog dcat:dataset must reference a valid register URI
            var catalogToDatasetTriples = graph.GetTriplesWithPredicate(dcatDatasetPredicate).ToList();

            catalogToDatasetTriples.Should().NotBeEmpty(
                "The catalog must reference at least one dataset via dcat:dataset");

            var catalogDatasetUris = catalogToDatasetTriples
                .Select(t => t.Object)
                .OfType<IUriNode>()
                .Select(n => n.Uri)
                .ToList();

            catalogDatasetUris.Should().Contain(expectedDatasetUri,
                $"The catalog must reference dataset URI {expectedDatasetUri}");

            // 2) The dcat:Dataset resource in the graph must have the same URI and be typed correctly
            var datasetUriNode = graph.CreateUriNode(expectedDatasetUri);

            graph.ContainsTriple(new Triple(
                datasetUriNode,
                rdfType,
                dcatDatasetType
            )).Should().BeTrue(
                $"The dataset resource {expectedDatasetUri} must be typed as dcat:Dataset");
        }
    }

    [Test]
    public async Task Given_catalogue_with_dataset_and_distribution_When_exporting_to_dcat_rdf_Then_distribution_accessService_URI_should_be_correct()
    {
        // Arrange

        _datasetsService.GetDataset(Arg.Any<Guid>()).Returns(ModelsHelper.DcatDatasetModel);

        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);

        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
              .Returns(new PagedResult<DcatCatalogRecordModel>()
              {
                  Page = 1,
                  PageSize = 1,
                  Results = [ModelsHelper.DcatCatalogRecordModelDataset],
                  TotalCount = 1
              });

        var handler = CreateHandler();
        var command = new ExportDcatCatalogCommand(Guid.NewGuid(), RdfExportFormat.RDF);

        // Act
        var rdfXml = await handler.Handle(command, CancellationToken.None);

        // Assert
        using (var graph = new Graph())
        {
            var parser = new RdfXmlParser();
            using var reader = new StringReader(rdfXml);
            parser.Load(graph, reader);

            var dcatAccessServicePredicate = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#accessService"));

            var distributionToAccessServiceTriples =
                graph.GetTriplesWithPredicate(dcatAccessServicePredicate).ToList();

            distributionToAccessServiceTriples.Should().NotBeEmpty(
                "the distribution should expose at least one access service in this test setup");

            var identifier = ModelsHelper.DataServiceModel.Identifiers.First();

            var expectedAccessServiceUri = new Uri(
                $"https://iri.i14y.d.c.bfs.admin.ch/dataservice/{identifier}",
                UriKind.Absolute);

            foreach (var accessServiceNode in distributionToAccessServiceTriples.Select(t => t.Object))
            {
                accessServiceNode.Should().BeAssignableTo<IUriNode>(
                    "dcat:accessService must point to a URI node");

                var uri = ((IUriNode)accessServiceNode).Uri;

                uri.ToString().Should().Be(
                    $"https://iri.i14y.d.c.bfs.admin.ch/dataservice/{identifier}",
                    "access service URIs must have the form https://iri.i14y.d.c.bfs.admin.ch/dataservice/{id}");
            }

            distributionToAccessServiceTriples
                .Select(t => t.Object)
                .OfType<IUriNode>()
                .Any(u => u.Uri == expectedAccessServiceUri)
                .Should().BeTrue(
                    $"the distribution must reference access service URI {expectedAccessServiceUri}");
        }
    }

    [Test]
    public async Task Given_catalogue_with_dataservice_When_exporting_to_dcat_rdf_Then_dataservice_URI_should_be_correct()
    {
        // Arrange
        _dataServicesService.GetDataService(Arg.Any<Guid>()).Returns(ModelsHelper.DataServiceModel);

        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);

        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
              .Returns(new PagedResult<DcatCatalogRecordModel>()
              {
                  Page = 1,
                  PageSize = 1,
                  Results = [ModelsHelper.DcatCatalogRecordModelDataservice],
                  TotalCount = 1
              });

        var handler = CreateHandler();
        var command = new ExportDcatCatalogCommand(Guid.NewGuid(), RdfExportFormat.RDF);

        // Act
        var rdfXml = await handler.Handle(command, CancellationToken.None);

        // Assert
        using (var graph = new Graph())
        {
            var parser = new RdfXmlParser();
            using var reader = new StringReader(rdfXml);
            parser.Load(graph, reader);

            var dcatServicePredicate = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#service"));

            var rdfTypePredicate = graph.CreateUriNode(
                UriFactory.Create(RdfSpecsHelper.RdfType));

            var dcatDataServiceType = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#DataService"));

            var catalogToServiceTriples =
                graph.GetTriplesWithPredicate(dcatServicePredicate).ToList();

            catalogToServiceTriples.Should().NotBeEmpty(
                "the catalog should expose at least one data service via dcat:service");

            var identifier = ModelsHelper.DataServiceModel.Id;

            var expectedDataServiceUri = new Uri(
                $"https://iri.i14y.d.c.bfs.admin.ch/dataservice/{identifier}",
                UriKind.Absolute);

            foreach (var dataServiceNode in catalogToServiceTriples.Select(t => t.Object))
            {
                dataServiceNode.Should().BeAssignableTo<IUriNode>(
                    "dcat:service must point to a URI node");

                var uri = ((IUriNode)dataServiceNode).Uri;

                uri.ToString().Should().Be(
                    $"https://iri.i14y.d.c.bfs.admin.ch/dataservice/{identifier}",
                    "data service URIs must have the form https://iri.i14y.d.c.bfs.admin.ch/dataservice/{id}");

                graph.ContainsTriple(new Triple(
                    dataServiceNode,
                    rdfTypePredicate,
                    dcatDataServiceType
                )).Should().BeTrue(
                    "each data service referenced by dcat:service must be typed as dcat:DataService");
            }

            catalogToServiceTriples
                .Select(t => t.Object)
                .OfType<IUriNode>()
                .Any(u => u.Uri == expectedDataServiceUri)
                .Should().BeTrue(
                    $"the catalog must reference data service URI {expectedDataServiceUri}");

        }
    }

    [Test]
    public async Task Given_distribution_coverage_with_datetimeoffset_When_exporting_to_dcat_rdf_Then_coverage_is_formatted_as_date_only()
    {
        // Arrange
        var distribution = ModelsHelper.DcatDatasetModel.Distributions!.First() with
        {
            Coverage =
            [
                new PeriodOfTimeModel
                {
                    Start = new DateTimeOffset(2026, 6, 17, 23, 0, 0, TimeSpan.FromHours(10))
                }
            ]
        };

        var dataset = ModelsHelper.DcatDatasetModel with
        {
            Distributions = [distribution]
        };

        _datasetsService.GetDataset(Arg.Any<Guid>()).Returns(dataset);

        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));

        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);

        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
              .Returns(new PagedResult<DcatCatalogRecordModel>()
              {
                  Page = 1,
                  PageSize = 1,
                  Results = [ModelsHelper.DcatCatalogRecordModelDataset],
                  TotalCount = 1
              });

        var handler = CreateHandler();
        var command = new ExportDcatCatalogCommand(Guid.NewGuid(), RdfExportFormat.RDF);

        // Act
        var rdfXml = await handler.Handle(command, CancellationToken.None);

        // Assert
        using (var graph = new Graph())
        {
            var parser = new RdfXmlParser();
            using var reader = new StringReader(rdfXml);
            parser.Load(graph, reader);

            var dcatDistributionPredicate = graph.CreateUriNode(
                UriFactory.Create("http://www.w3.org/ns/dcat#distribution"));

            var dctCoveragePredicate = graph.CreateUriNode(
                UriFactory.Create("http://purl.org/dc/terms/coverage"));

            var coverageValues = graph
                .GetTriplesWithPredicate(dcatDistributionPredicate)
                .Select(t => t.Object)
                .SelectMany(distributionNode => graph.GetTriplesWithSubjectPredicate(distributionNode, dctCoveragePredicate))
                .Select(t => t.Object)
                .OfType<ILiteralNode>()
                .Select(node => node.Value)
                .ToList();

            coverageValues.Should().ContainSingle();
            coverageValues.Single().Should().Be("2026-06-17");
        }
    }

    [TestCase(RdfExportFormat.RDF)]
    [TestCase(RdfExportFormat.TTL)]
    public async Task Given_distribution_urls_with_unicode_characters_When_exporting_to_dcat_Then_unicode_characters_are_preserved(RdfExportFormat format)
    {
        // Arrange
        const string unicodeUrl = "https://www.bfe-ogd.ch/ogd105_heizgradtage_kühlgradtage.csv";
        const string percentEncodedUrl = "https://www.bfe-ogd.ch/export?label=été%20chaud";
        var distribution = ModelsHelper.DcatDistributionModel with
        {
            AccessUrl = new ResourceModel { Uri = unicodeUrl },
            DownloadUrl = new ResourceModel { Uri = percentEncodedUrl },
            AccessServices = []
        };
        var dataset = ModelsHelper.DcatDatasetModel with
        {
            Distributions = [distribution]
        };

        _datasetsService.GetDataset(Arg.Any<Guid>()).Returns(dataset);
        _datasetsService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));
        _dataServicesService.GetUserAllowActionInfo(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<AllowActionResult>>([ModelsHelper.AllowActionResultRead]));
        _catalogService.GetDcatCatalog(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ModelsHelper.DcatCatalogModel);
        _catalogService.GetDcatCatalogRecords(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<DcatCatalogRecordModel>
            {
                Page = 1,
                PageSize = 1,
                Results = [ModelsHelper.DcatCatalogRecordModelDataset],
                TotalCount = 1
            });

        var handler = CreateHandler();

        // Act
        var export = await handler.Handle(new ExportDcatCatalogCommand(Guid.NewGuid(), format), CancellationToken.None);

        // Assert
        using var _ = new AssertionScope();
        var expectedUrls = format switch
        {
            RdfExportFormat.RDF => new[]
            {
                $"rdf:resource=\"{unicodeUrl}\"", $"rdf:resource=\"{percentEncodedUrl}\""
            },
            RdfExportFormat.TTL => new[] { $"<{unicodeUrl}>", $"<{percentEncodedUrl}>" },
            _ => Array.Empty<string>()
        };

        foreach (var expectedUrl in expectedUrls)
        {
            export.Should().Contain(expectedUrl);
        }

        export.Should().NotContain("k%C3%BChlgradtage");
        export.Should().Contain("%20");
    }

    private ExportDcatCatalogCommandHandler CreateHandler()
    {
        var i14yOptions = Microsoft.Extensions.Options.Options.Create(new I14YOptions
        {
            IriBaseUrl = "https://iri.i14y.d.c.bfs.admin.ch"
        });

        return new ExportDcatCatalogCommandHandler(
            _catalogService,
            _datasetsService,
            _dataServicesService,
            i14yOptions,
            _logger
        );
    }
}