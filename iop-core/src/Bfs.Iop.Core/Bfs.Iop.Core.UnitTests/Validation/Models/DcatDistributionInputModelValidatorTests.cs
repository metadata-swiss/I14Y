using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(DcatDistributionInputModelValidator))]
public sealed class DcatDistributionInputModelValidatorTests
{
    [Test]
    public void Given_model_with_repeated_ids_in_accessServices_When_Validate_Then_Fail()
    {
        // Arrange
        var id = Guid.NewGuid();
        var idModel1 = new IdModel() { Id = id };
        var idModel2 = new IdModel() { Id = id };

        var model = ModelsHelper.DcatDistributionInputModel with
        {
            AccessServices = [idModel1, idModel2]
        };

        var subject = CreateValidator();

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.First().ErrorMessage.Should().Be("The collection cannot contain repeated ids.");
    }

    private static DcatDistributionInputModelValidator CreateValidator() =>
        new(TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<EuPlannedAvailabilityVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<FileTypesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<Iso639LanguagesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<LicenseTypesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<MediaTypesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<PackingFormatsVocabulary>(),
            new InlineValidator<ResourceModel>(),
            new InlineValidator<ChecksumInputModel>(),
            new InlineValidator<PeriodOfTimeModel>(),
            Substitute.For<IDataServicesService>());
}
