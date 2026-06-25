using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[Category(nameof(DatasetQualityInformation))]
public class DatasetQualityInformationMappingProfileTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_QualityInfo_EntityToModel_Ok()
    {
        var infoEntity = TestData.Core.QualityInformationModel;
        var infoModel = TestData.Model.QualityInformation;

        var mappedInfoModel = _mapper.Map<DatasetQualityInformationModel>(infoEntity);
        mappedInfoModel.Should().BeEquivalentTo(infoModel);
    }

    [Test]
    public void Map_QualityInfo_ModelToEntity_Ok()
    {
        var infoEntity = TestData.Core.QualityInformationModel;
        var infoModel = TestData.Model.QualityInformation;

        var mappedInfoEntity = _mapper.Map<DatasetQualityInformation>(infoModel);
        mappedInfoEntity.Should().BeEquivalentTo(infoEntity);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}