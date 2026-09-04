using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Models;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(VCardInputModelValidator))]
internal sealed class VCardInputModelValidatorTests
{
    [Test]
    public void Given_contact_point_without_kind_When_validate_Then_defaults_to_organization_and_validation_ok()
    {
        // Arrange
        var model = new VCardModel
        {
            HasEmail = "contact@example.org"
        };

        var subject = new VCardInputModelValidator();

        // Act
        var result = subject.Validate(model);

        // Assert
        model.Kind.Should().Be(VCardKind.Organization);
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_contact_point_with_invalid_kind_When_validate_Then_fails()
    {
        // Arrange
        var model = new VCardModel
        {
            HasEmail = "contact@example.org",
            Kind = (VCardKind)0
        };

        var subject = new VCardInputModelValidator();

        // Act
        var result = subject.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(VCardModel.Kind));
    }
}
