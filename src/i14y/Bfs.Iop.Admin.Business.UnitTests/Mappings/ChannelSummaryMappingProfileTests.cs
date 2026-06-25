using AutoMapper;
using AwesomeAssertions;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class ChannelSummaryMappingProfileTests
{
    private IMapper _mapper = null!;

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
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}