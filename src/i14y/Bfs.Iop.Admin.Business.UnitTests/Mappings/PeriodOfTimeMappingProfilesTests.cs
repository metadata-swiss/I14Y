using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class PeriodOfTimeMappingProfilesTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_PeriodOfTime_IopAdminModelToDcat_Ok()
    {
        // Arrange
        var source = TestData.Model.TemporalCoverage;

        // Act
        var result = _mapper.Map<PeriodOfTimeModel>(source);

        // Assert
        result.End.Should().Be(source.End);
        result.Start.Should().Be(source.Start);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}