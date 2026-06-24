using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Models;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(ResourceInputModelValidator))]
internal sealed class ResourceInputModelValidatorTests
{
    [TestCase("", false)]
    [TestCase("toto", false)]
    [TestCase("http://www.toto.ch", true)]
    public void Given_mode_with_uri_When_validating_Then_expected(string uri, bool expected)
    {
        // Arrange
        var model = new ResourceModel()
        {
            Uri = uri
        };
        var validator = new ResourceInputModelValidator();

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().Be(expected);
    }
}
