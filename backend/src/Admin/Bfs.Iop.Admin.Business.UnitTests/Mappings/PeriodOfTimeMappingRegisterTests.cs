using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Core.Abstractions.Models;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(PeriodOfTimeMappingRegister))]
public class PeriodOfTimeMappingRegisterTests
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
    public void Setup() => _mapper = TestHelper.CreateMapper();
}