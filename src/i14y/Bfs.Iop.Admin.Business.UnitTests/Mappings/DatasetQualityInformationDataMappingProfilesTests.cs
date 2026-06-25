using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[Category(nameof(DatasetQualityInformationData))]
public class DatasetQualityInformationDataMappingProfilesTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_QualityData_EntityToModel_Ok()
    {
        var dataDcat = TestData.Core.QualityInformationDataModel;
        var dataModel = TestData.Model.QualityInformationData;

        var mappedDataModel = _mapper.Map<DatasetQualityInformationDataModel>(dataDcat);
        mappedDataModel.Should().BeEquivalentTo(dataModel);
    }

    [Test]
    public void Map_QualityData_ModelToDcat_Ok()
    {
        var dataDcat = TestData.Core.QualityInformationDataModel;
        var dataModel = TestData.Model.QualityInformationData;

        var mappedDataDcat = _mapper.Map<DatasetQualityInformationDataModel>(dataModel);
        mappedDataDcat.Should().BeEquivalentTo(dataDcat);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}