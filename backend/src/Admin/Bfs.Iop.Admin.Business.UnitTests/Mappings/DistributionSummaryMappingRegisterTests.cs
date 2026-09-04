using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DistributionSummaryMappingRegister))]
public class DistributionSummaryMappingRegisterTests
{
    private IMapper _mapper = null!;

    [TestCaseSource(nameof(GetDistributionTestCases))]
    public void Map_DistributionSummary_DcatToIopAdminModel_Ok(DcatDistributionModel source)
    {
        // Act
        var result = _mapper.Map<Models.DistributionSummary>(source);

        // Assert
        using (new AssertionScope())
        {
            result.Id.Should().Be(source.Id);
            result.Format.Should().BeEquivalentTo(source.Format);
            result.LastUpdated.Should().Be(source.Modified);
            result.Published.Should().Be(source.Issued);
            result.Title.De.Should().Be(source.Title.De);
        }
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();

    private static IEnumerable<TestCaseData> GetDistributionTestCases()
    {
        var subject1 = TestData.Core.Distribution;

        var subject2 = subject1 with { Format = null };

        var subjects = new[]
        {
            subject1,
            subject2
        };

        for (int i = 0; i < subjects.Length; i++)
        {
            yield return new TestCaseData(subjects[i]).SetArgDisplayNames($"TestCase{i + 1}");
        }
    }
}