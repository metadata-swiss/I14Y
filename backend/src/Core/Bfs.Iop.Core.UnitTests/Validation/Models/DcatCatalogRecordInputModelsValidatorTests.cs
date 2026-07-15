using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using FluentValidation;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(DcatCatalogRecordInputModelsValidator))]
internal class DcatCatalogRecordInputModelsValidatorTests
{
    [Test]
    public void Given_models_with_repeated_primary_topic_When_validating_Then_should_return_validation_error()
    {
        // Arrange
        var dataService = EntitiesHelper.DataService;

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            }
        };

        var subject = CreateFakeValidator();

        // Act 
        var result = subject.Validate([model, model]);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.First().ErrorMessage.Should().Be("Only one catalog record per resource is allowed.");
    }

    private static DcatCatalogRecordInputModelsValidator CreateFakeValidator() =>
        new(new InlineValidator<DcatCatalogRecordInputModel>());
}
