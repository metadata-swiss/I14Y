using Bfs.Iop.Core.Mappings;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.DataAccess.Relational.Tests.Helpers;

namespace Bfs.Iop.Core.UnitTests.Mappings;

[TestFixture(TestOf = typeof(AnnotationMappingExtensions))]
internal sealed class AnnotationMappingExtensionsTests
{
    [Test]
    public void Given_Annotation_When_MapToAnnotationModel_Then_Mapping_Ok()
    {
        // Arrange
        var subject = EntitiesHelper.Annotation;

        // Act
        var result = subject.MapToAnnotationModel();

        // Assert
        using var _ = new AssertionScope();
        result.Id.Should().Be(subject.Id);
        result.Identifier.Should().Be(subject.Identifier);
        result.Text?.En.Should().Be(subject.Text?.En);
        result.Type.Should().Be(subject.Type);
        result.Uri.Should().Be(subject.Uri);
    }
}
