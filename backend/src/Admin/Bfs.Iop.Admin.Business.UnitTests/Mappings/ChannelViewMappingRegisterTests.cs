using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(ChannelViewMappingRegister))]
public class ChannelViewMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DcatChannel_To_IopChannelView_Ok()
    {
        // Arrange
        var source = TestData.Core.ChannelModel;

        // Act
        var result = _mapper.Map<Models.ChannelView>(source);

        // Assert
        result.Description.De.Should().Be(source.Description.De);
        result.Identifier.Should().Be(source.Identifier);
        result.OpeningHours.Should().Be(source.OpeningHours);
        result.Type.Code.Should().Be(source.Type.Code);
        result.Type.Name.De.Should().Be(source.Type.Name.De);
    }

    [Test]
    public void Map_DcatChannel_To_IopChannelSummary_Ok()
    {
        // Arrange
        var source = TestData.Core.ChannelModel;

        // Act
        var result = _mapper.Map<Models.ChannelSummary>(source);

        // Assert
        result.Identifier.Should().Be(source.Identifier);
        result.Type.De.Should().Be(source.Type.Name.De);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}