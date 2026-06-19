using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.Core.LinkedData.Helpers;
using Bfs.Iop.Core.LinkedData.Services;
using AwesomeAssertions;
using Newtonsoft.Json;
using NSubstitute;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Core.FileStorage;

namespace Bfs.Iop.Core.LinkedData.UnitTests;

[TestFixture(TestOf = typeof(DatasetModelProcessHelper))]
internal class DatasetModelProcessServiceTests
{
    private DatasetModelFileProcessService _fileService;
    private IFileStorageService _fileStorageService;

    [SetUp]
    public void Setup()
    {
        _fileStorageService = Substitute.For<IFileStorageService>();
        _fileService = new DatasetModelFileProcessService(
            _fileStorageService,
            new ApiSettings { EnvironmentName = "DEV" },
            Substitute.For<IDatasetsService>());
    }

    [TestCase]
    public async Task Given_DatasetId_When_CovertGraphToSchemaGraph_Then_Return_expected()
    {
        // Arrange
        Guid datasetId = new Guid("0de392f5-a1cc-4a38-b739-ff230b192ed4");
        var cancellationToken = new CancellationToken();

        string jsonExpectedResult = "{'schemaName':'https://www.i14y.admin.ch/resources/datasets/0de392f5-a1cc-4a38-b739-ff230b192ed4/','classes':[{'label':{'en':'Root'},'description':{'en':'eCH-0108'},'uriComplete':'http://i14y.admin.ch/ns#RootShape','closed':true,'properties':[{'path':'http://i14y.admin.ch/ns#localUnitMasterData','allowedValues':[],'description':{},'label':{'en':'localUnitMasterData'},'uriComplete':'http://i14y.admin.ch/ns#localUnitMasterData','toClassUri':'http://i14y.admin.ch/ns#localUnitMasterDataType','minCardinality':0,'minLength':0}]},{'label':{'en':'localUnitMasterDataType'},'description':{},'uriComplete':'http://i14y.admin.ch/ns#localUnitMasterDataType','closed':true,'properties':[{'path':'http://i14y.admin.ch/ns#additionalPostOfficeBoxAddress','allowedValues':[],'description':{},'label':{'en':'additionalPostOfficeBoxAddress'},'uriComplete':'http://i14y.admin.ch/ns#additionalPostOfficeBoxAddress','toClassUri':'http://i14y.admin.ch/ns#additionalPostOfficeBoxAddressType','minCardinality':0,'minLength':0},{'path':'http://i14y.admin.ch/ns#name','allowedValues':[],'description':{},'label':{'en':'name'},'uriComplete':'http://i14y.admin.ch/ns#name','minCardinality':1,'minLength':1,'maxLength':255},{'path':'http://i14y.admin.ch/ns#statusDate','allowedValues':[],'description':{},'label':{'en':'statusDate'},'dataType':':string','uriComplete':'http://i14y.admin.ch/ns#statusDate','minCardinality':0,'minLength':0},{'path':'http://i14y.admin.ch/ns#additionalName','allowedValues':[],'description':{},'label':{'en':'additionalName'},'uriComplete':'http://i14y.admin.ch/ns#additionalName','minCardinality':0,'minLength':1,'maxLength':255},{'path':'http://i14y.admin.ch/ns#localId','allowedValues':[],'description':{},'label':{'en':'localId'},'pattern':'^[A-B][1-9][0-9]{7}$','uriComplete':'http://i14y.admin.ch/ns#localId','minCardinality':0,'minLength':9,'maxLength':9},{'path':'http://i14y.admin.ch/ns#mainUid','allowedValues':[],'description':{},'label':{'en':'mainUid'},'pattern':'^CHE[1-9][0-9]{8}$','uriComplete':'http://i14y.admin.ch/ns#mainUid','minCardinality':0,'minLength':12,'maxLength':12}]},{'label':{'en':'additionalPostOfficeBoxAddressType'},'description':{},'uriComplete':'http://i14y.admin.ch/ns#additionalPostOfficeBoxAddressType','closed':true,'properties':[{'path':'http://i14y.admin.ch/ns#addressCategory','allowedValues':[],'description':{},'label':{'en':'addressCategory'},'dataType':':string','uriComplete':'http://i14y.admin.ch/ns#addressCategory','minCardinality':1,'minLength':0}]}]}";
        var expectedResult = JsonConvert.DeserializeObject<SchemaGraph>(jsonExpectedResult);
        _fileStorageService.DownloadAsync("", "").ReturnsForAnyArgs(GetStoredFile(datasetId));

        // Act
        var result = _fileService.GetDatasetModelGraphAsync(datasetId, cancellationToken).GetAwaiter().GetResult();

        // Assert
        result.Classes.Should().HaveCount(expectedResult!.Classes!.Count());
        result.SchemaName.Should().Be(expectedResult.SchemaName);
        var classExpected = expectedResult!.Classes!.First();
        var classResult = result!.Classes!.Where(c => c.UriComplete.AbsoluteUri == classExpected.UriComplete.AbsoluteUri)!.First();
        classResult.Label.En.Should().Be(classExpected.Label.En);
        var propertyExpected = classExpected.Properties.FirstOrDefault();
        var propertyResult = classResult!.Properties!.Where(p => p.UriComplete?.Fragment == propertyExpected.UriComplete?.Fragment)!.First();
        propertyResult.Label.En.Should().Be(propertyExpected.Label.En);
    }

    private static Task<StoredFile> GetStoredFile(Guid datasetId)
    {
        var filename = $"Resources/{datasetId}.ttl";

        var renderingFileStream = File.OpenText(filename);

        var metadata = new StoredFileMetadata(filename, "", renderingFileStream.BaseStream.Length, DateTimeOffset.Now);

        return Task.FromResult(new StoredFile(renderingFileStream.BaseStream, metadata));
    }
}