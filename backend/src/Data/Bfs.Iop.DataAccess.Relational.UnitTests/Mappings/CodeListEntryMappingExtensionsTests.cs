using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;

namespace Bfs.Iop.Core.UnitTests.Mappings;

[TestFixture(TestOf = typeof(CodeListEntryMappingExtensions))]
internal sealed class CodeListEntryMappingExtensionsTests
{
    [Test]
    public void Given_CodeListEntry_When_MapToCodeListEntryModel_Then_Mapping_Ok()
    {
        // Arrange
        var subject = EntitiesHelper.CodeListEntry;

        // Act
        var result = subject.MapToCodeListEntryModel();

        // Assert
        using var _ = new AssertionScope();
        result.Annotations.Should().HaveCount(subject.Annotations.Count());
        result.Annotations.First().Id.Should().Be(subject.Annotations.First().Id);
        result.Code.Should().Be(subject.Code);
        result.Description?.En.Should().Be(subject.Description?.En);
        result.Id.Should().Be(subject.Id);
        result.Name.En.Should().Be(subject.Name.En);
    }
}
