using Bfs.Iop.Core.Data.Contracts;
using static Bfs.Iop.Core.LinkedData.UnitTests.TestServiceProvider;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.Core.LinkedData.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Bfs.Iop.Core.Settings;

namespace Bfs.Iop.Core.UnitTests.LinkedData;

[TestFixture(TestOf = typeof(DatasetModelFileProcessService))]
internal class DatasetModelFileProcessServiceTests
{
    private DatasetModelFileProcessService _modelFileProcessService;
    private IFileStorageService _storeService;

    [SetUp]
    public void Setup()
    {
        _storeService = Substitute.For<IFileStorageService>();
        _modelFileProcessService = new DatasetModelFileProcessService(
            _storeService,
            new ApiSettings { EnvironmentName = "DEV" },
            DatasetsServiceProvider());
    }

    [TestCase]
    public void Given_DatasetId_and_ImportFileHaveNoNodes_When_UploadFile_Then_throw_exception()
    {
        // Arrange
        Guid datasetId = new Guid("0de392f5-a1cc-4a38-b739-ff230b192ef9");
        var cancellationToken = new CancellationToken();
        IFormFile fileUpload;
        var stream = File.OpenRead($"Resources/EmptyNodeFile.rdf");
        fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

        // Act
        var action = () => _modelFileProcessService.UploadGraph(fileUpload, datasetId, cancellationToken).GetAwaiter().GetResult();
        // Assert
        action.Should().ThrowExactly<InvalidOperationException>("The file does not contain valid information");

        stream.Dispose();
    }
}
