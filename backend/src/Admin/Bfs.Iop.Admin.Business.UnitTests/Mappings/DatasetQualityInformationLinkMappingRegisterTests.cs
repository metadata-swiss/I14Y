using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Admin.Models;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DatasetQualityInformationLinkMappingRegister))]
public class DatasetQualityInformationLinkMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_QualityInfo_EntityToModel_Ok()
    {
        var infoEntity = TestData.Core.QualityInformationLinkModel;
        var infoModel = TestData.Model.QualityInformationLink;

        var mappedInfoModel = _mapper.Map<DatasetQualityInformationLink>(infoEntity);
        mappedInfoModel.Should().BeEquivalentTo(infoModel);
    }

    [Test]
    public void Map_QualityInfo_ModelToEntity_Ok()
    {
        var infoEntity = TestData.Core.QualityInformationLinkModel;
        var infoModel = TestData.Model.QualityInformationLink;

        var mappedInfoEntity = _mapper.Map<DatasetQualityInformationLink>(infoModel);
        mappedInfoEntity.Should().BeEquivalentTo(infoEntity);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}