using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Mappings;

[TestFixture(TestOf = typeof(MultiLanguageMappingExtensions))]
internal sealed class MultiLanguageMappingExtensionsTests
{
    [Test]
    public void Given_Multilanguage_When_MapToMultiLanguageModel_Then_Mapping_Ok()
    {
        // Arrange
        var subject = EntitiesHelper.MultiLanguage;

        // Act
        var result = subject.MapToMultiLanguageModel();

        // Assert
        using var _ = new AssertionScope();
        result.De.Should().Be(subject.De);
        result.En.Should().Be(subject.En);
        result.Fr.Should().Be(subject.Fr);
        result.It.Should().Be(subject.It);
        result.Rm.Should().Be(subject.Rm);
    }

    [Test]
    public void Given_MultilanguageModel_When_MapToMultiLanguage_Then_Mapping_Ok()
    {
        // Arrange
        var subject = ModelsHelper.MultiLanguageModel;

        // Act
        var result = subject.MapToMultiLanguage();

        // Assert
        using var _ = new AssertionScope();
        result.De.Should().Be(subject.De);
        result.En.Should().Be(subject.En);
        result.Fr.Should().Be(subject.Fr);
        result.It.Should().Be(subject.It);
        result.Rm.Should().Be(subject.Rm);
    }
}
