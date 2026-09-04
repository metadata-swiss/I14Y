using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(DcatDatasetInputModelValidator))]
internal sealed class DcatDatasetInputModelValidatorTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void Setup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void Given_model_with_repeated_identifiers_When_Validate_Then_Fail()
    {
        // Arrange
        var model = ModelsHelper.DcatDatasetInputModel with
        {
            Identifiers = ["123", "123"] 
        };

        var subject = CreateFakeValidator(_dbContext);

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.First().ErrorMessage.Should().Contain(model.Identifiers.First());
    }

    [Test]
    public async Task Given_model_with_same_identifier_as_entity_When_validating_Then_fails()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var model = ModelsHelper.DcatDatasetInputModel with 
        { 
            Identifiers = [fakeIdentifier] 
        };

        var dataset = new Dataset()
        { 
            Identifier = [fakeIdentifier] 
        };

        _dbContext.Datasets.Add(dataset);
        _dbContext.SaveChanges();

        var validator = CreateFakeValidator(_dbContext);

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().Be(false);
    }

    [Test]
    public async Task Given_updateid_and_model_with_same_identifier_as_entity_When_validating_Then_validation_ok()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var model = ModelsHelper.DcatDatasetInputModel with
        {
            Identifiers = [fakeIdentifier]
        };

        var dataset = new Dataset()
        {
            Identifier = [fakeIdentifier]
        };

        _dbContext.Datasets.Add(dataset);
        _dbContext.SaveChanges();

        var validator = CreateFakeValidator(_dbContext);

        // Act
        var context = new ValidationContext<DcatDatasetInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.IdKey] = dataset.Id;

        var result = await validator.ValidateAsync(context);

        // Assert
        result.IsValid.Should().Be(true);
    }

    [Test]
    public async Task Given_updateid_and_model_with_same_Identifier_of_another_entity_When_validating_Then_validation_fails()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var model = ModelsHelper.DcatDatasetInputModel with
        {
            Identifiers = [fakeIdentifier]
        };

        var dataset = new Dataset()
        {
            Identifier = [fakeIdentifier]
        };

        _dbContext.Datasets.Add(dataset);
        _dbContext.SaveChanges();

        var validator = CreateFakeValidator(_dbContext);

        // Act
        var context = new ValidationContext<DcatDatasetInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.IdKey] = Guid.NewGuid();

        var result = await validator.ValidateAsync(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.First().PropertyName.Should().Be(nameof(model.Identifiers) + "[0]");
    }

    private static DcatDatasetInputModelValidator CreateFakeValidator(IopDbContext iopDbContext) =>
        new(
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<RightsStatementsVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ConfidentialityPersonsVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<FrequencyTypesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<GeoIvIdsVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<Iso639LanguagesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ThemesVocabulary>(),
            new InlineValidator<ResourceModel>(),
            new InlineValidator<DcatQualifiedAttributionInputModel>(),
            new InlineValidator<DcatQualifiedRelationInputModel>(),
            new InlineValidator<DcatDistributionInputModel>(),
            new InlineValidator<PeriodOfTimeModel>(),
            new InlineValidator<VCardModel>(),
            new InlineValidator<KeywordModel>(),
            iopDbContext);
}
