using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Models;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(PeriodOfTimeInputModelValidator))]
internal sealed class PeriodOfTimeModelValidatorTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void Given_dateOnlyPeriodOfTime_When_validating_Then_expected(PeriodOfTimeModel subject, bool expected)
    {
        // Arrange
        var validator = new PeriodOfTimeInputModelValidator();

        // Act
        var result = validator.Validate(subject);

        // Assert
        result.IsValid.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new TestCaseData(
            new PeriodOfTimeModel()
            {
                Start = new DateTimeOffset(2000, 10, 1, 10, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2000, 9, 1, 10, 0, 0, 0, TimeSpan.Zero)
            },
            false).SetArgDisplayNames("Test 1");

        yield return new TestCaseData(
            new PeriodOfTimeModel()
            {
                Start = new DateTimeOffset(2000, 10, 1, 10, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2000, 10, 1, 10, 0, 0, TimeSpan.Zero),
            },
            true).SetArgDisplayNames("Test 2");

        yield return new TestCaseData(
            new PeriodOfTimeModel()
            {
                Start = new DateTimeOffset(2000, 10, 1, 10, 0, 0, TimeSpan.Zero),
                End = new DateTimeOffset(2000, 11, 1, 10, 0, 0, TimeSpan.Zero),
            },
            true).SetArgDisplayNames("Test 3");

        yield return new TestCaseData(
            new PeriodOfTimeModel()
            {
                Start = null,
                End = new DateTimeOffset(2000, 11, 1, 10, 0, 0, TimeSpan.Zero)
            },
            true).SetArgDisplayNames("Test 4");

        yield return new TestCaseData(
            new PeriodOfTimeModel()
            {
                Start = null,
                End = null
            },
            false).SetArgDisplayNames("Test 5");
    }
}
