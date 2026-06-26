using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(IdLabelMappingProfiles))]
public class IdLabelMappingProfileTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DataServiceModel_ToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.DataServiceModel;

        // Act
        var result = _mapper.Map<Models.IdLabel>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.Label.De.Should().Be(source.Title.De);
        result.Label.Fr.Should().Be(source.Title.Fr);
        result.Label.It.Should().Be(source.Title.It);
        result.Label.En.Should().Be(source.Title.En);
    }

    [Test]
    public void Map_Dataset_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.DcatDatasetModel;

        // Act
        var result = _mapper.Map<Models.IdLabel>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.Label.De.Should().Be(source.Title.De);
        result.Label.Fr.Should().Be(source.Title.Fr);
        result.Label.It.Should().Be(source.Title.It);
        result.Label.En.Should().Be(source.Title.En);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}